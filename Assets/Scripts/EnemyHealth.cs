using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Health Bar")]
    [SerializeField] private Image healthFill;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathDelay = 2f;

    private float currentHealth;
    private WaveManager waveManager;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Start()
    {
        currentHealth = maxHealth;

        waveManager = FindFirstObjectByType<WaveManager>();

        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0f);

        UpdateHealthBar();

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
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount =
                currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Enemy Died");

        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }

        if (waveManager != null)
        {
            waveManager.EnemyKilled();
        }

        Destroy(gameObject, deathDelay);
    }
}