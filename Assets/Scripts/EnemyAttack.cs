
using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public enum AttackType
    {
        None,
        ForceField,
        Object,
        Player
    }

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 2.5f;

    [Header("Target Selection")]
    [SerializeField] private float playerTargetWeight = 1f;
    [SerializeField] private float objectTargetWeight = 1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string forceFieldAttackTrigger = "Attack1";
    [SerializeField] private string objectAttackTrigger = "Attack1";
    [SerializeField] private string playerAttackTrigger = "Attack2";
    [SerializeField] private float attackAnimationDuration = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private EnemyMovement enemyMovement;
    private Stronghold targetStronghold;

    private AttackType currentAttackType = AttackType.None;
    private StrongholdObject attackObject;
    private PlayerHealth attackPlayer;
    private Transform currentAttackTarget;

    private float attackTimer;
    private bool isAttacking;
    private bool damageApplied;
    private Coroutine attackCoroutine;

    public bool IsAttacking => isAttacking;
    public Stronghold TargetStronghold => targetStronghold;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        attackTimer = 0f;

        if (showDebugMessages)
            Debug.Log(name + " | EnemyAttack initialized.");
    }

    private void Update()
    {
        if (targetStronghold == null || targetStronghold.IsDestroyed)
            return;

        if (isAttacking)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        // The force field must be destroyed before the stronghold
        // objects and players become valid attack targets.
        if (!targetStronghold.IsBreached)
        {
            attackObject = null;
            attackPlayer = null;
            currentAttackTarget = null;

            TryAttackForceField();
            return;
        }

        // Keep a valid target instead of selecting a new one every frame.
        if (!IsCurrentTargetValid())
            ChoosePostBreachTarget();

        // The enemy may already be moving towards its selected target.
        if (currentAttackTarget == null)
            return;

        if (!IsInRange(currentAttackTarget))
            return;

        if (currentAttackType == AttackType.Player &&
            attackPlayer != null)
        {
            BeginAttack(AttackType.Player, attackPlayer.transform);
            TriggerAnimation(playerAttackTrigger);
        }
        else if (currentAttackType == AttackType.Object &&
                 attackObject != null)
        {
            BeginAttack(AttackType.Object, attackObject.transform);
            TriggerAnimation(objectAttackTrigger);
        }
    }

    public void SetTargetStronghold(Stronghold stronghold)
    {
        if (targetStronghold == stronghold)
            return;

        targetStronghold = stronghold;

        attackObject = null;
        attackPlayer = null;
        currentAttackTarget = null;
        currentAttackType = AttackType.None;
        attackTimer = 0f;

        if (enemyMovement != null)
        {
            if (stronghold != null)
                enemyMovement.SetTarget(stronghold);
            else
                enemyMovement.ClearTarget();
        }

        if (showDebugMessages)
        {
            string targetName = stronghold != null
                ? stronghold.name
                : "None";

            Debug.Log(name + " | Assigned target stronghold: " + targetName);
        }
    }

    private void TryAttackForceField()
    {
        if (targetStronghold == null ||
            targetStronghold.IsDestroyed ||
            targetStronghold.IsBreached)
            return;

        Transform fieldTarget =
            targetStronghold.transform.Find("EnemyTarget");

        if (fieldTarget == null)
            fieldTarget = targetStronghold.transform;

        if (!IsInRange(fieldTarget))
            return;

        BeginAttack(AttackType.ForceField, fieldTarget);
        TriggerAnimation(forceFieldAttackTrigger);
    }

    private void ChoosePostBreachTarget()
    {
        if (targetStronghold == null || targetStronghold.IsDestroyed)
            return;

        StrongholdObject closestObject =
            targetStronghold.GetClosestAvailableObject(transform.position);

        PlayerHealth closestPlayer = FindClosestLivingPlayer();

        bool hasObject = closestObject != null;
        bool hasPlayer = closestPlayer != null;

        if (!hasObject && !hasPlayer)
            return;

        bool choosePlayer;

        if (hasObject && hasPlayer)
        {
            float playerWeight = Mathf.Max(0f, playerTargetWeight);
            float objectWeight = Mathf.Max(0f, objectTargetWeight);
            float totalWeight = playerWeight + objectWeight;

            if (totalWeight <= 0f)
            {
                choosePlayer = Random.value < 0.5f;
            }
            else
            {
                choosePlayer = Random.value <
                    playerWeight / totalWeight;
            }
        }
        else
        {
            choosePlayer = hasPlayer;
        }

        if (choosePlayer)
            SetPlayerTarget(closestPlayer);
        else
            SetObjectTarget(closestObject);
    }

    private void SetObjectTarget(StrongholdObject targetObject)
    {
        if (targetObject == null || targetObject.IsDestroyed)
            return;

        attackObject = targetObject;
        attackPlayer = null;
        currentAttackTarget = targetObject.transform;
        currentAttackType = AttackType.Object;

        if (enemyMovement != null)
            enemyMovement.SetTargetObject(targetObject);

        if (showDebugMessages)
            Debug.Log(name + " | Selected object target: " + targetObject.name);
    }

    private void SetPlayerTarget(PlayerHealth player)
    {
        if (player == null || player.IsDead)
            return;

        attackPlayer = player;
        attackObject = null;
        currentAttackTarget = player.transform;
        currentAttackType = AttackType.Player;

        if (enemyMovement != null)
            enemyMovement.SetTargetPlayer(player.transform);

        if (showDebugMessages)
            Debug.Log(name + " | Selected player target: " + player.name);
    }

    private bool IsCurrentTargetValid()
    {
        if (currentAttackTarget == null)
            return false;

        if (currentAttackType == AttackType.Object)
        {
            return attackObject != null &&
                   !attackObject.IsDestroyed;
        }

        if (currentAttackType == AttackType.Player)
        {
            return attackPlayer != null &&
                   !attackPlayer.IsDead;
        }

        return false;
    }

    private void BeginAttack(AttackType attackType, Transform attackTarget)
    {
        if (isAttacking || attackTarget == null)
            return;

        isAttacking = true;
        damageApplied = false;

        currentAttackType = attackType;
        currentAttackTarget = attackTarget;

        if (enemyMovement != null &&
            enemyMovement.Agent != null &&
            enemyMovement.Agent.isOnNavMesh)
        {
            enemyMovement.StopMoving();
        }

        FaceTarget(attackTarget);
        ApplyDamageImmediately();

        attackCoroutine = StartCoroutine(FinishAttackAfterDelay());
    }

    private void ApplyDamageImmediately()
    {
        if (damageApplied)
            return;

        damageApplied = true;

        switch (currentAttackType)
        {
            case AttackType.ForceField:
                if (targetStronghold != null &&
                    !targetStronghold.IsDestroyed)
                {
                    targetStronghold.DamageForceField(attackDamage);
                }
                break;

            case AttackType.Object:
                if (attackObject != null &&
                    !attackObject.IsDestroyed)
                {
                    attackObject.TakeDamage(attackDamage);
                }
                break;

            case AttackType.Player:
                if (attackPlayer != null && !attackPlayer.IsDead)
                {
                    attackPlayer.TakeDamage(attackDamage);
                }
                break;
        }
    }

    // Kept for existing Animation Events.
    // Damage is handled by BeginAttack().
    public void ApplyAttackDamage()
    {
    }

    private void TriggerAnimation(string triggerName)
    {
        if (animator == null)
        {
            if (showDebugMessages)
                Debug.LogWarning(name + " | No Animator assigned.");

            return;
        }

        if (string.IsNullOrWhiteSpace(triggerName))
            return;

        animator.SetTrigger(triggerName);
    }

    private IEnumerator FinishAttackAfterDelay()
    {
        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        currentAttackType = AttackType.None;
        attackCoroutine = null;

        attackTimer = attackInterval;

        if (enemyMovement != null)
            enemyMovement.ResumeMoving();

        // Keep the selected target if it remains valid.
        // A new target is chosen only when necessary.
        if (!IsCurrentTargetValid())
        {
            attackObject = null;
            attackPlayer = null;
            currentAttackTarget = null;
        }
    }

    private bool IsInRange(Transform target)
    {
        if (target == null)
            return false;

        return Vector3.Distance(
            transform.position,
            target.position
        ) <= attackRange;
    }

    private void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private PlayerHealth FindClosestLivingPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        PlayerHealth closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject playerObject in players)
        {
            if (playerObject == null)
                continue;

            PlayerHealth health =
                playerObject.GetComponent<PlayerHealth>();

            if (health == null || health.IsDead)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                playerObject.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = health;
            }
        }

        return closestPlayer;
    }
}
