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

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackInterval;
        }
    }

    private void Attack()
    {
        if (targetStronghold.IsBreached)
            return;

        targetStronghold.DamageForceField(attackDamage);
    }
}