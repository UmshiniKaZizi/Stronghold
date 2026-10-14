using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Assigned Stronghold")]
    [SerializeField] private Stronghold associatedStronghold;

    private float currentHealth;
    private bool hasDied;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => hasDied || currentHealth <= 0f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        Debug.Log(name + " Health: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    // Called when the player dies from enemy damage.
    private void Die()
    {
        if (hasDied)
            return;

        hasDied = true;

        Debug.Log("PLAYER DIED | " + name);

        // Destroy the player's associated stronghold.
        if (associatedStronghold != null &&
            !associatedStronghold.IsDestroyed)
        {
            associatedStronghold.DestroyStronghold("Player died");
        }

        HandleDeathConsequences();
    }

    // Called when the player's stronghold is destroyed.
    public void DieFromStrongholdDestruction()
    {
        if (IsDead)
            return;

        hasDied = true;
        currentHealth = 0f;

        Debug.Log(
            "PLAYER DIED | " + name +
            " | Reason: Associated stronghold destroyed"
        );

        HandleDeathConsequences();
    }

    private void HandleDeathConsequences()
    {
        // Switch to a surviving character before checking game over.
        CharacterSwap characterSwap =
            FindFirstObjectByType<CharacterSwap>();

        if (characterSwap != null)
        {
            characterSwap.OnCharacterDied(this);
        }

        // Preserve the intentional two-death game-over rule.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }

        // Disable the dead character.
        gameObject.SetActive(false);
    }
}