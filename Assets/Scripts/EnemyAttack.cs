using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Stronghold targetStronghold;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 2f;

    private float attackTimer;

    private EnemyMovement enemyMovement;

    private void Awake()
    {
        enemyMovement =
            GetComponent<EnemyMovement>();

        if (enemyMovement == null)
        {
            Debug.LogError(
                gameObject.name +
                " is missing EnemyMovement!"
            );
        }
    }

    private void Update()
    {
        if (targetStronghold == null)
            return;

        attackTimer -= Time.deltaTime;

        // ==========================================
        // FORCE FIELD
        // ==========================================

        if (!targetStronghold.IsBreached)
        {
            AttackForceField();
            return;
        }

        // ==========================================
        // CHARACTER
        // ==========================================

        AttackCharacter();
    }


    // ==========================================
    // ATTACK FORCE FIELD
    // ==========================================

    private void AttackForceField()
    {
        if (targetStronghold == null)
            return;

        if (targetStronghold.IsBreached)
            return;

        if (enemyMovement == null)
            return;

        Transform enemyTarget =
            enemyMovement.CurrentTarget;

        if (enemyTarget == null)
            return;

        // ==========================================
        // CHECK DISTANCE TO ENEMY TARGET
        // ==========================================

        float distance =
            Vector3.Distance(
                transform.position,
                enemyTarget.position
            );

        // Enemy has NOT reached the stronghold yet
        if (distance > attackRange)
            return;

        // ==========================================
        // ATTACK COOLDOWN
        // ==========================================

        if (attackTimer > 0f)
            return;

        Debug.Log(
            "FORCE FIELD ATTACK | " +
            gameObject.name +
            " → " +
            targetStronghold.name +
            " | Distance: " +
            distance.ToString("F2")
        );

        targetStronghold.DamageForceField(
            attackDamage
        );

        attackTimer = attackInterval;
    }


    // ==========================================
    // ATTACK CHARACTER
    // ==========================================

    private void AttackCharacter()
    {
        if (enemyMovement == null)
            return;

        Transform target =
            enemyMovement.CurrentTarget;

        if (target == null)
            return;

        if (!target.gameObject.activeSelf)
            return;

        PlayerHealth playerHealth =
            target.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (playerHealth.IsDead)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        if (distance > attackRange)
            return;

        if (attackTimer > 0f)
            return;

        playerHealth.TakeDamage(
            attackDamage
        );

        Debug.Log(
            "ENEMY ATTACK | " +
            gameObject.name +
            " → " +
            target.name +
            " | Damage: " +
            attackDamage
        );

        attackTimer = attackInterval;
    }


    // ==========================================
    // SET STRONGHOLD TARGET
    // ==========================================

    public void SetTargetStronghold(
        Stronghold stronghold)
    {
        targetStronghold = stronghold;

        if (stronghold == null)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Target Stronghold is NULL!"
            );

            if (enemyMovement != null)
            {
                enemyMovement.ClearTarget();
            }

            return;
        }

        Debug.Log(
            "TARGET ASSIGNED | " +
            gameObject.name +
            " → " +
            stronghold.name
        );

        // ==========================================
        // IMPORTANT:
        // ASSIGN THE SAME TARGET TO MOVEMENT
        // ==========================================

        if (enemyMovement != null)
        {
            enemyMovement.SetTarget(
                stronghold
            );
        }
    }


    // ==========================================
    // OPTIONAL PUBLIC GETTER
    // ==========================================

    public Stronghold CurrentTargetStronghold
    {
        get
        {
            return targetStronghold;
        }
    }
}