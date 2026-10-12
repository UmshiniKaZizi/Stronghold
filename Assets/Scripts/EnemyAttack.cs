
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

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackParameterDuration = 0.2f;

    private float attackTimer;
    private EnemyMovement enemyMovement;
    private bool isAttacking;

    private Coroutine attack1ResetCoroutine;
    private Coroutine attack2ResetCoroutine;

    private static readonly int Attack1Hash =
        Animator.StringToHash("Attack1");

    private static readonly int Attack2Hash =
        Animator.StringToHash("Attack2");

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (enemyMovement == null)
        {
            Debug.LogError(
                gameObject.name +
                " is missing EnemyMovement!"
            );
        }

        if (animator == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " has no Animator assigned or found in children."
            );
        }
    }

    private void Update()
    {
        if (targetStronghold == null)
            return;

        attackTimer -= Time.deltaTime;

        // Attack the force field while it is intact.
        if (!targetStronghold.IsBreached)
        {
            AttackForceField();
            return;
        }

        // Do not start another player attack during a lunge.
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

        Transform target = enemyMovement.CurrentTarget;

        if (target == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        if (distance > attackRange)
            return;

        if (attackTimer > 0f)
            return;

        // Play Attack1 for a force-field attack.
        PlayAttackAnimation(Attack1Hash);

        Debug.Log(
            "FORCE FIELD ATTACK | " +
            gameObject.name +
            " → " +
            targetStronghold.name
        );

        targetStronghold.DamageForceField(attackDamage);

        attackTimer = attackInterval;
    }

    // ==========================================
    // CHARACTER ATTACK
    // ==========================================

    private void AttackCharacter()
    {
        if (enemyMovement == null)
            return;

        Transform target = enemyMovement.CurrentTarget;

        if (target == null)
            return;

        PlayerHealth playerHealth = GetPlayerHealth(target);

        if (playerHealth == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " cannot find PlayerHealth on " +
                target.name
            );

            return;
        }

        if (playerHealth.IsDead)
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        if (distance > attackRange)
            return;

        if (attackTimer > 0f)
            return;

        StartCoroutine(
            LungeAttack(target, playerHealth)
        );
    }

    private PlayerHealth GetPlayerHealth(Transform character)
    {
        if (character == null)
            return null;

        PlayerHealth health =
            character.GetComponent<PlayerHealth>();

        if (health == null)
            health = character.GetComponentInParent<PlayerHealth>();

        if (health == null)
            health = character.GetComponentInChildren<PlayerHealth>();

        return health;
    }

    // ==========================================
    // LUNGE ATTACK
    // ==========================================

    private IEnumerator LungeAttack(
        Transform target,
        PlayerHealth playerHealth)
    {
        isAttacking = true;

        // Play Attack2 when attacking a player.
        PlayAttackAnimation(Attack2Hash);

        // Stop navigation during the lunge.
        if (enemyMovement != null &&
            enemyMovement.Agent != null)
        {
            enemyMovement.Agent.isStopped = true;
        }

        // Face the player.
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }

        Vector3 startPosition = transform.position;

        Vector3 lungePosition =
            startPosition + transform.forward * lungeDistance;

        // Move forward.
        while (Vector3.Distance(
                   transform.position,
                   lungePosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                lungePosition,
                lungeSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Deal damage if the player is still alive
        // and remains within reach.
        if (target != null &&
            playerHealth != null &&
            !playerHealth.IsDead)
        {
            float distance = Vector3.Distance(
                transform.position,
                target.position
            );

            if (distance <= attackRange + lungeDistance)
            {
                playerHealth.TakeDamage(attackDamage);

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

        // Return to the starting position.
        while (Vector3.Distance(
                   transform.position,
                   startPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                returnSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = startPosition;

        // Reset the attack cooldown.
        attackTimer = attackInterval;
        isAttacking = false;

        // Give navigation control back.
        if (enemyMovement != null &&
            enemyMovement.Agent != null)
        {
            enemyMovement.Agent.isStopped = false;
        }
    }

    // ==========================================
    // ANIMATION
    // ==========================================

    private void PlayAttackAnimation(int parameterHash)
    {
        if (animator == null)
            return;

        // Restart the reset timer if this parameter
        // is already active.
        if (parameterHash == Attack1Hash)
        {
            if (attack1ResetCoroutine != null)
                StopCoroutine(attack1ResetCoroutine);

            animator.SetBool(Attack1Hash, true);

            attack1ResetCoroutine = StartCoroutine(
                ResetAttackParameter(
                    Attack1Hash,
                    true
                )
            );
        }
        else if (parameterHash == Attack2Hash)
        {
            if (attack2ResetCoroutine != null)
                StopCoroutine(attack2ResetCoroutine);

            animator.SetBool(Attack2Hash, true);

            attack2ResetCoroutine = StartCoroutine(
                ResetAttackParameter(
                    Attack2Hash,
                    false
                )
            );
        }
    }

    private IEnumerator ResetAttackParameter(
        int parameterHash,
        bool isAttack1)
    {
        yield return new WaitForSeconds(
            attackParameterDuration
        );

        if (animator != null)
            animator.SetBool(parameterHash, false);

        if (isAttack1)
            attack1ResetCoroutine = null;
        else
            attack2ResetCoroutine = null;
    }

    // ==========================================
    // SET STRONGHOLD TARGET
    // ==========================================

    public void SetTargetStronghold(Stronghold stronghold)
    {
        targetStronghold = stronghold;

        if (targetStronghold == null)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Target Stronghold is NULL!"
            );

            if (enemyMovement != null)
                enemyMovement.ClearTarget();

            return;
        }

        Debug.Log(
            "TARGET ASSIGNED | " +
            gameObject.name +
            " → " +
            targetStronghold.name
        );

        if (enemyMovement != null)
            enemyMovement.SetTarget(targetStronghold);
    }

    // ==========================================
    // CURRENT TARGET
    // ==========================================

    public Stronghold CurrentTargetStronghold
    {
        get { return targetStronghold; }
    }
}
