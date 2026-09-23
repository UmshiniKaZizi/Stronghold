using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Stronghold targetStronghold;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 8f;

    [Header("Attack Movement")]
    [SerializeField] private float lungeDistance = 1.0f;
    [SerializeField] private float lungeSpeed = 8f;
    [SerializeField] private float returnSpeed = 6f;

    private float attackTimer;

    private EnemyMovement enemyMovement;

    private bool isAttacking;

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

        if (!isAttacking)
        {
            AttackCharacter();
        }
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

        // Not close enough
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

        attackTimer =
            attackInterval;
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
            return;

        // ==========================================
        // FIND PLAYER HEALTH
        // ==========================================

        PlayerHealth playerHealth =
            target.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth =
                target.GetComponentInParent<PlayerHealth>();
        }

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

        // Player is dead
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
        // START VISUAL ATTACK
        // ==========================================

        StartCoroutine(
            LungeAttack(
                target,
                playerHealth
            )
        );
    }


    // ==========================================
    // LUNGE ATTACK
    // ==========================================

    private IEnumerator LungeAttack(
        Transform target,
        PlayerHealth playerHealth)
    {
        isAttacking = true;

        // ==========================================
        // STOP NAVIGATION
        // ==========================================

        if (enemyMovement != null &&
            enemyMovement.Agent != null)
        {
            enemyMovement.Agent.isStopped = true;
        }

        // ==========================================
        // FACE PLAYER
        // ==========================================

        Vector3 direction =
            target.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction
                );

            transform.rotation =
                targetRotation;
        }

        // ==========================================
        // STORE START POSITION
        // ==========================================

        Vector3 startPosition =
            transform.position;

        // ==========================================
        // CALCULATE LUNGE POSITION
        // ==========================================

        Vector3 forward =
            transform.forward;

        Vector3 lungePosition =
            startPosition +
            forward * lungeDistance;

        // ==========================================
        // LUNGE FORWARD
        // ==========================================

        while (
            Vector3.Distance(
                transform.position,
                lungePosition
            ) > 0.05f)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    lungePosition,
                    lungeSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        // ==========================================
        // DEAL DAMAGE
        // ==========================================

        if (target != null &&
            playerHealth != null &&
            !playerHealth.IsDead)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    target.position
                );

            // Only damage if we're still reasonably
            // close to the player.
            if (distance <= attackRange + lungeDistance)
            {
                playerHealth.TakeDamage(
                    attackDamage
                );

                Debug.Log(
                    "PLAYER ATTACK | " +
                    gameObject.name +
                    " → " +
                    playerHealth.gameObject.name +
                    " | Damage: " +
                    attackDamage
                );
            }
        }

        // ==========================================
        // RETURN
        // ==========================================

        while (
            Vector3.Distance(
                transform.position,
                startPosition
            ) > 0.05f)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    startPosition,
                    returnSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        transform.position =
            startPosition;

        // ==========================================
        // RESET COOLDOWN
        // ==========================================

        attackTimer =
            attackInterval;

        isAttacking = false;

        // ==========================================
        // GIVE NAVMESH CONTROL BACK
        // ==========================================

        if (enemyMovement != null &&
            enemyMovement.Agent != null)
        {
            enemyMovement.Agent.isStopped = false;
        }
    }


    // ==========================================
    // SET STRONGHOLD TARGET
    // ==========================================

    public void SetTargetStronghold(
        Stronghold stronghold)
    {
        targetStronghold =
            stronghold;

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

        if (enemyMovement != null)
        {
            enemyMovement.SetTarget(
                targetStronghold
            );
        }
    }


    // ==========================================
    // CURRENT STRONGHOLD
    // ==========================================

    public Stronghold CurrentTargetStronghold
    {
        get
        {
            return targetStronghold;
        }
    }
}