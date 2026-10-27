using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Assigned Stronghold")]
    [SerializeField] private Stronghold associatedStronghold;

    private float currentHealth;
    private bool hasDied;
    private bool healthInitialized;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => hasDied || currentHealth <= 0f;

    private void Awake()
    {
        // Safe default so IsDead is never true merely because Start has not run yet.
        InitializeHealth();
    }

    /// <summary>Called by GameManager before CharacterSwap builds its linked list.</summary>
    public void InitializeHealth()
    {
        currentHealth = Mathf.Max(1f, maxHealth);
        hasDied = false;
        healthInitialized = true;

        Debug.Log($"PLAYER HEALTH INITIALIZED | {name} | {currentHealth}/{maxHealth} | IsDead: {IsDead}");
    }

    public void TakeDamage(float damage)
    {
        if (!healthInitialized)
            InitializeHealth();

        if (IsDead || damage <= 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        Debug.Log(name + " Health: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (hasDied)
            return;

        hasDied = true;
        Debug.Log("PLAYER DIED | " + name);

        if (associatedStronghold != null && !associatedStronghold.IsDestroyed)
            associatedStronghold.DestroyStronghold("Player died");

        HandleDeathConsequences();
    }

    public void DieFromStrongholdDestruction()
    {
        if (IsDead)
            return;

        hasDied = true;
        currentHealth = 0f;

        Debug.Log("PLAYER DIED | " + name + " | Reason: Associated stronghold destroyed");
        HandleDeathConsequences();
    }

    private void HandleDeathConsequences()
    {
        CharacterSwap characterSwap = FindFirstObjectByType<CharacterSwap>();

        if (characterSwap != null)
        {
            Debug.LogError(
                "BEFORE CHARACTER DEATH HANDLER | Swap Active: " + characterSwap.gameObject.activeInHierarchy +
                " | Parent: " + (characterSwap.transform.parent != null ? characterSwap.transform.parent.name : "No Parent"));

            characterSwap.OnCharacterDied(this);
            characterSwap.RefreshSwitchingInput();

            Debug.LogError("AFTER CHARACTER DEATH HANDLER | Swap Active: " + characterSwap.gameObject.activeInHierarchy);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();

        // This deactivates only the character that died, not the CharacterSwap manager.
        gameObject.SetActive(false);
    }
}
