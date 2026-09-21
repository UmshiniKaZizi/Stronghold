using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Stronghold targetStronghold;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 1f;

    private float attackTimer;

    private void Update()
    {
        if (targetStronghold == null)
            return;

        attackTimer -= Time.deltaTime;

        if (!targetStronghold.IsBreached)
        {
            AttackForceField();
        }
        else
        {
            // Character targeting will happen here later.
            // For now, the enemy stops attacking the force field.
        }
    }

    private void AttackForceField()
    {
        if (attackTimer > 0f)
            return;

        Debug.Log(
            "ENEMY ATTACK | " +
            gameObject.name +
            " → " +
            targetStronghold.name
        );

        targetStronghold.DamageForceField(
            attackDamage
        );

        attackTimer = attackInterval;
    }

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

            return;
        }

        Debug.Log(
            "TARGET ASSIGNED | " +
            gameObject.name +
            " → " +
            targetStronghold.name
        );
    }
}