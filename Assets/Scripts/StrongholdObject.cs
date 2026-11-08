
using UnityEngine;
using UnityEngine.Events;

public class StrongholdObject : MonoBehaviour
{
    [Header("Object Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private GameObject destroyedVisual;
    [SerializeField] private Collider[] hitColliders;
    [SerializeField] private Renderer[] intactRenderers;

    private Stronghold stronghold;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDestroyed => CurrentHealth <= 0f;
    public UnityEvent<float, float> HealthChanged = 
        new UnityEvent<float, float>();

    public UnityEvent<StrongholdObject, float> Damaged = 
        new UnityEvent<StrongholdObject, float>();

    public UnityEvent<StrongholdObject> Destroyed = 
        new UnityEvent<StrongholdObject>();

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);

        if (hitColliders == null || hitColliders.Length == 0)
            hitColliders = GetComponentsInChildren<Collider>(true);

        if (intactRenderers == null || intactRenderers.Length == 0)
            intactRenderers = GetComponentsInChildren<Renderer>(true);

        CurrentHealth = maxHealth;
        SetIntact(true);
    }

    //Connects this object to its owning stronghold.
    public void SetStronghold(Stronghold owner)
    {
        stronghold = owner;
    }

    public void TakeDamage(float amount)
    {
        if (IsDestroyed || amount <= 0f) return;
        
        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        float actualDamage = previousHealth - CurrentHealth;
        
        HealthChanged.Invoke(CurrentHealth, maxHealth);
        Damaged.Invoke(this, actualDamage);
        if (IsDestroyed)
        {
            SetIntact(false);
            Destroyed.Invoke(this);
        }
    }

    // ==========================================
    // FULL RESTORATION
    // ==========================================

    // Returns true only when an actual restoration succeeds.
    public bool RepairToFull()
    {
        if (CurrentHealth >= maxHealth)
            return false;

        // Capture the Stronghold's previous state before
        // changing this object's health.
        StrongholdTier previousTier =
            stronghold != null
                ? stronghold.Tier
                : StrongholdTier.Secure;

        CurrentHealth = maxHealth;

        // A destroyed object needs its visuals and colliders
        // reactivated. A partially damaged object does not.
        if (IsDestroyed == false)
        {
            SetIntact(true);
        }

        HealthChanged.Invoke(CurrentHealth, maxHealth);

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
    // VISUALS AND COLLIDERS
    // ==========================================

    private void SetIntact(bool intact)
    {
        foreach (Collider item in hitColliders)
        {
            if (item != null)
                item.enabled = intact;
        }

        foreach (Renderer item in intactRenderers)
        {
            if (item != null &&
                (destroyedVisual == null ||
                 !item.transform.IsChildOf(destroyedVisual.transform)))
            {
                item.enabled = intact;
            }
        }

        if (destroyedVisual != null)
            destroyedVisual.SetActive(!intact);
    }


  //  [Header("Destruction")]
  //  [SerializeField] private GameObject destructionEffect;
   // [SerializeField] private bool destroyObjectOnDeath = true;

   // private float currentHealth;
   // 
   // private bool isDestroyed;

   // public float CurrentHealth => currentHealth;
   // public float MaxHealth => maxHealth;
   // public bool IsDestroyed => isDestroyed;


  //  public void TakeDamage(float damage)
   // {
      //  if (isDestroyed || damage <= 0f)
      //      return;

       // currentHealth = Mathf.Max(
        //    currentHealth - damage,
        //    0f
       // );

       // Debug.Log(
        //    "OBJECT DAMAGED | " + name +
        //    " | Health: " + currentHealth +
         //   "/" + maxHealth
        //);

       // if (currentHealth <= 0f)
       // {
        //    Die();
       // }
    //}

   // private void Die()
  //  {
   //     if (isDestroyed)
   //         return;
//
     //   isDestroyed = true;

      //  Debug.Log("OBJECT DESTROYED | " + name);

      //  if (stronghold != null)
      //  {
      //     stronghold.RegisterObjectDestroyed(this);
       // }
       // else
       // {
       //     Debug.LogWarning(
        //        name + " has no Stronghold assigned."
        //    );
       // }

       // if (destructionEffect != null)
      //  {
       //     Instantiate(
       //         destructionEffect,
        //        transform.position,
         //       transform.rotation
          //  );
       // }

        //if (destroyObjectOnDeath)
      //  {
      //      Destroy(gameObject);
      //  }
       // else
       // {
            // Keep the object visible but disable interaction.
         //   foreach (Collider objectCollider in
         //            GetComponentsInChildren<Collider>())
         //   {
          //      objectCollider.enabled = false;
          //  }

          //  foreach (Renderer objectRenderer in
          //           GetComponentsInChildren<Renderer>())
          //  {
          //      objectRenderer.enabled = false;
          //  }
      //  }
   // }
}
