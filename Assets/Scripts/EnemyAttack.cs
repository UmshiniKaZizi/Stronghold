using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Stronghold targetStronghold;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 8f;

    private float attackTimer;

    private EnemyMovement enemyMovement;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();

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
    // FORCE FIELD ATTACK
    // ==========================================

    private void AttackForceField()
    {
        if (targetStronghold == null)
            return;

        if (targetStronghold.IsBreached)
            return;

        if (enemyMovement == null)
            return;

        Transform target =
            enemyMovement.CurrentTarget;

        if (target == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        // Enemy has not reached the stronghold yet
        if (distance > attackRange)
            return;

        if (attackTimer > 0f)
            return;

        Debug.Log(
            "FORCE FIELD ATTACK | " +
            gameObject.name +
            " → " +
            targetStronghold.name
        );

        targetStronghold.DamageForceField(
            attackDamage
        );

        attackTimer = attackInterval;
    }


    // ==========================================
    // CHARACTER ATTACK
    // ==========================================

    private void AttackCharacter()
    {
        if (enemyMovement == null)
            return;

        Transform target =
            enemyMovement.CurrentTarget;

        if (target == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " has no character target."
            );

            return;
        }

        // ==========================================
        // FIND PLAYER HEALTH
        // ==========================================

        PlayerHealth playerHealth =
            target.GetComponent<PlayerHealth>();

        // If PlayerHealth is on a parent
        if (playerHealth == null)
        {
            playerHealth =
                target.GetComponentInParent<PlayerHealth>();
        }

        // If PlayerHealth is on a child
        if (playerHealth == null)
        {
            playerHealth =
                target.GetComponentInChildren<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " cannot find PlayerHealth on " +
                target.name
            );

            return;
        }

        // ==========================================
        // CHECK PLAYER
        // ==========================================

        if (!target.gameObject.activeSelf)
            return;

        if (playerHealth.IsDead)
            return;

        // ==========================================
        // DISTANCE
        // ==========================================

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        if (distance > attackRange)
            return;

        // ==========================================
        // COOLDOWN
        // ==========================================

        if (attackTimer > 0f)
            return;

        // ==========================================
        // DAMAGE
        // ==========================================

        playerHealth.TakeDamage(
            attackDamage
        );

        Debug.Log(
            "PLAYER HIT | " +
            gameObject.name +
            " → " +
            playerHealth.gameObject.name +
            " | Damage: " +
            attackDamage +
            " | Distance: " +
            distance.ToString("F2")
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

        if (targetStronghold == null)
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
            targetStronghold.name
        );

        // Make sure movement has the same target
        if (enemyMovement != null)
        {
            enemyMovement.SetTarget(
                targetStronghold
            );
        }
    }


    // ==========================================
    // GET CURRENT STRONGHOLD
    // ==========================================

    public Stronghold CurrentTargetStronghold
    {
        get
        {
            return targetStronghold;
        }
    }
}