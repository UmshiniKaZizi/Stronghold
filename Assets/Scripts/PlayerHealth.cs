using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;

    public bool IsDead => currentHealth <= 0f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(
            currentHealth,
            0f
        );

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
        Debug.Log(
            gameObject.name +
            " DIED"
        );

        // Disable the entire character
        gameObject.SetActive(false);
    }
}