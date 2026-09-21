using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private WaveManager waveManager;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Start()
    {
        currentHealth = maxHealth;

        waveManager =
            FindFirstObjectByType<WaveManager>();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        currentHealth =
            Mathf.Max(
                currentHealth,
                0f
            );

        Debug.Log(
            "Enemy Health: " +
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
    Debug.Log("Enemy Died");

    if (waveManager != null)
    {
        waveManager.EnemyKilled();
    }

    Destroy(gameObject);
}
}