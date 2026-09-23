using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool hasDied = false;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (hasDied)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(
            gameObject.name +
            " Health: " +
            currentHealth +
            "/" +
            maxHealth
        );

        if (IsDead)
        {
            Die();
        }
    }

    private void Die()
    {
        if (hasDied)
            return;

        hasDied = true;

        Debug.Log(gameObject.name + " DIED");

        // Tell GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }

        // Disable the character
        gameObject.SetActive(false);
    }
}