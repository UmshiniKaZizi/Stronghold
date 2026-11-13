
using UnityEditor.Experimental.GraphView;
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
    private bool hasDied;

    private GameObject lastAttackingPlayer;

    private WaveManager waveManager;
    private EnemyMovement enemyMovement;
    private ZombieAudio zombieAudio;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => hasDied || currentHealth <= 0f;

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

    private static readonly int DeadHash =
        Animator.StringToHash("Dead");

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        zombieAudio = GetComponent<ZombieAudio>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        waveManager = FindFirstObjectByType<WaveManager>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(float damage, PlayerHealth attacker)
    {
        if (attacker != null && !attacker.IsDead)
        {
            lastAttackingPlayer = attacker.gameObject;
        }
        

        if (IsDead || damage <= 0f)
            return;

        currentHealth = Mathf.Max(
            currentHealth - damage,
            0f
        );

        UpdateHealthBar();

        Debug.Log(
            name + " Health: " +
            currentHealth + "/" + maxHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            // Play zombie hit sound.
            if (zombieAudio != null)
            {
                zombieAudio.PlayHitSound();
            }

            // Play hit animation.
            if (animator != null)
            {
                animator.SetTrigger(HitHash);
            }
        }

       // if (currentHealth <= 0f)
        //{
        //    Die();
      //  }
       // else if (animator != null)
      //  {
       //     animator.SetTrigger(HitHash);
       // }
    }

    private void UpdateHealthBar()
    {
        if (healthFill == null)
        {
            Debug.LogWarning(
                name + ": Health Fill Image is not assigned.",
                this
            );
            return;
        }

        healthFill.fillAmount = maxHealth > 0f
            ? currentHealth / maxHealth
            : 0f;
    }

    private void Die()
    {
        if (hasDied)
            return;

        hasDied = true;

        // Play zombie death audio.
        if (zombieAudio != null)
        {
            zombieAudio.PlayDeathSound();
        }

        Debug.Log(name + " DIED");

        // Capture the assigned stronghold before destruction.
        Stronghold assignedStronghold =
            enemyMovement != null
                ? enemyMovement.CurrentStronghold
                : null;

        // Notify the WaveManager exactly once.
        if (waveManager != null)
        {
            waveManager.EnemyKilled(assignedStronghold, lastAttackingPlayer);
        }
        else
        {
            Debug.LogWarning(
                name + ": WaveManager was not found."
            );
        }

        // Play the death animation.
        if (animator != null)
            animator.SetTrigger(DeadHash);

        // Disable movement and combat.
        if (enemyMovement != null)
            enemyMovement.enabled = false;

        EnemyAttack enemyAttack =
            GetComponent<EnemyAttack>();

        if (enemyAttack != null)
            enemyAttack.enabled = false;

        // Allow the death animation to finish.
        Destroy(gameObject, deathDelay);
    }
}
