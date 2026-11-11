using UnityEngine;
using UnityEngine.Events;

public class StrongholdObject : MonoBehaviour
{
    [Header("Object Health")]
    [SerializeField] private float maxHealth = 100f;
    [Header("Enemy Target")]
    [SerializeField] private Transform enemyTarget;

    [Header("Damage Behaviour")]
    [SerializeField] private DamageableObject damageableObject;

    [Header("Legacy Object Visuals")]
    [SerializeField] private GameObject destroyedVisual;
    [SerializeField] private Collider[] hitColliders;
    [SerializeField] private Renderer[] intactRenderers;

    private Stronghold stronghold;

    public float CurrentHealth { get; private set; }

    public float MaxHealth => maxHealth;

    public Transform EnemyTarget =>
    enemyTarget != null
        ? enemyTarget
        : transform;

    public bool IsDestroyed =>
        CurrentHealth <= 0f;

    public UnityEvent<float, float> HealthChanged =
        new UnityEvent<float, float>();

    public UnityEvent<StrongholdObject, float> Damaged =
        new UnityEvent<StrongholdObject, float>();

    public UnityEvent<StrongholdObject> Destroyed =
        new UnityEvent<StrongholdObject>();

    private void Awake()
    {
        maxHealth =
            Mathf.Max(1f, maxHealth);

        if (damageableObject == null)
        {
            damageableObject =
                GetComponent<DamageableObject>();
        }

        if (hitColliders == null ||
            hitColliders.Length == 0)
        {
            hitColliders =
                GetComponentsInChildren<Collider>(true);
        }

        if (intactRenderers == null ||
            intactRenderers.Length == 0)
        {
            intactRenderers =
                GetComponentsInChildren<Renderer>(true);
        }

        CurrentHealth = maxHealth;

        // Old StrongholdObjects can still
        // use the original visual system.
        //
        // New damageable objects control their
        // own normal/damaged/rubble visuals.
        if (damageableObject == null)
        {
            SetIntact(true);
        }
    }

    // Connects this object to its owning Stronghold.
    public void SetStronghold(Stronghold owner)
    {
        stronghold = owner;
    }

    // ==========================================
    // ORIGINAL DAMAGE CALL
    // ==========================================
    //
    // Kept so your existing zombie scripts
    // do not immediately break.
    public void TakeDamage(float amount)
    {
        Vector3 hitPoint =
            transform.position +
            Vector3.up * 0.5f;

        TakeDamage(
            amount,
            DamageSource.ZombieAttack,
            hitPoint,
            transform.forward
        );
    }

    // ==========================================
    // FULL DAMAGE CALL
    // ==========================================

    public void TakeDamage(
        float amount,
        DamageSource source,
        Vector3 hitPoint,
        Vector3 hitDirection)
    {
        if (IsDestroyed ||
            amount <= 0f)
        {
            return;
        }

        // Zombie bombs cannot damage
        // Stronghold objects.
        if (source == DamageSource.ZombieBomb)
        {
            Debug.Log(
                name +
                " ignored Zombie Bomb damage."
            );

            return;
        }

        float previousHealth =
            CurrentHealth;

        CurrentHealth =
            Mathf.Max(
                0f,
                CurrentHealth - amount
            );

        float actualDamage =
            previousHealth - CurrentHealth;

        Debug.Log(
            "STRONGHOLD OBJECT DAMAGED | " +
            name +
            " | Source: " +
            source +
            " | Health: " +
            CurrentHealth +
            "/" +
            maxHealth
        );

        HealthChanged.Invoke(
            CurrentHealth,
            maxHealth
        );

        Damaged.Invoke(
            this,
            actualDamage
        );

        // Tell DamageableObject to do
        // the physical/visual reaction.
        if (damageableObject != null)
        {
            damageableObject.HandleDamage(
                CurrentHealth,
                maxHealth,
                source,
                hitPoint,
                hitDirection
            );
        }

        if (IsDestroyed)
        {
            // Only use the old visual system
            // when this is NOT one of the new
            // DamageableObjects.
            if (damageableObject == null)
            {
                SetIntact(false);
            }

            Destroyed.Invoke(this);
        }
    }

    // ==========================================
    // FULL RESTORATION
    // ==========================================

    public bool RepairToFull()
    {
        if (CurrentHealth >= maxHealth)
            return false;

        StrongholdTier previousTier =
            stronghold != null
                ? stronghold.Tier
                : StrongholdTier.Secure;

        CurrentHealth = maxHealth;

        // Legacy objects can restore their
        // original visuals normally.
        if (damageableObject == null)
        {
            SetIntact(true);
        }

        HealthChanged.Invoke(
            CurrentHealth,
            maxHealth
        );

        if (stronghold != null)
        {
            stronghold.NotifyObjectRepaired(
                this,
                previousTier
            );
        }

        return true;
    }

    // ==========================================
    // LEGACY VISUALS AND COLLIDERS
    // ==========================================

    private void SetIntact(bool intact)
    {
        foreach (Collider item in hitColliders)
        {
            if (item != null)
            {
                item.enabled = intact;
            }
        }

        foreach (Renderer item in intactRenderers)
        {
            if (item != null &&
                (destroyedVisual == null ||
                 !item.transform.IsChildOf(
                     destroyedVisual.transform)))
            {
                item.enabled = intact;
            }
        }

        if (destroyedVisual != null)
        {
            destroyedVisual.SetActive(!intact);
        }
    }
}