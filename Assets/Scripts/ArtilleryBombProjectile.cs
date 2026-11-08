
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ArtilleryBombProjectile : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float maximumDamage = 100f;
    [SerializeField] private float minimumDamage = 25f;
    [SerializeField] private LayerMask damageMask = ~0;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 10f;

    private Rigidbody rb;
    private bool hasExploded;

    //07November addition
    private PlayerHealth ownerPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 velocity, Transform owner)
    {
        ownerPlayer = owner.GetComponentInParent<PlayerHealth>();

        
        rb.linearVelocity = velocity;

        if (owner == null)
            return;

        Collider[] projectileColliders =
            GetComponentsInChildren<Collider>();

        Collider[] ownerColliders =
            owner.GetComponentsInChildren<Collider>();

        foreach (Collider projectileCollider in projectileColliders)
        {
            foreach (Collider ownerCollider in ownerColliders)
            {
                if (projectileCollider != null &&
                    ownerCollider != null)
                {
                    Physics.IgnoreCollision(
                        projectileCollider,
                        ownerCollider
                    );
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        Debug.Log("ARTILLERY BOMB EXPLODED!");

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius,
            damageMask
        );

        HashSet<EnemyHealth> damagedEnemies =
            new HashSet<EnemyHealth>();

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy =
                hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null ||
                enemy.IsDead ||
                !damagedEnemies.Add(enemy))
            {
                continue;
            }

            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            float damagePercentage =
                1f - Mathf.Clamp01(distance / explosionRadius);

            float damage = Mathf.Lerp(
                minimumDamage,
                maximumDamage,
                damagePercentage
            );

            enemy.TakeDamage(damage, ownerPlayer);

            Debug.Log(
                $"Explosion damaged {enemy.name} for {damage:F1}"
            );
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}
