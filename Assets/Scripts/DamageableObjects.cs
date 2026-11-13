using UnityEngine;

public enum DamageSource
{
    ZombieAttack,
    PlayerBullet,
    ZombieBomb
}

public class DamageableObject : MonoBehaviour
{
    [Header("Stronghold Object")]
    [SerializeField] private StrongholdObject strongholdObject;

    [Header("Object Visuals")]
    [SerializeField] private GameObject normalVisual;
    [SerializeField] private GameObject damagedState;
    [SerializeField] private GameObject damagedVisual;
    [SerializeField] private GameObject damagePieces;

    [Header("Destroyed State")]
    [SerializeField] private GameObject destroyedRubble;

    [Header("Original Object Physics")]
    [SerializeField] private Rigidbody objectRigidbody;

    [Header("Damage Threshold")]
    [Range(0f, 1f)]
    [SerializeField] private float damagedHealthPercent = 0.6f;

    [Header("Damage Piece Physics")]
    [SerializeField] private float damagePieceForce = 1f;
    [SerializeField] private float damagePieceUpwardForce = 0.2f;
    [SerializeField] private float damagePieceTorque = .5f;

    [Header("Destroyed Rubble Physics")]
    [SerializeField] private float rubbleForce = 1f;
    [SerializeField] private float rubbleUpwardForce = 0.15f;
    [SerializeField] private float rubbleTorque = 0.5f;

    [Header("Debris Settings")]
    [SerializeField] private string debrisLayerName = "Debris";
    [SerializeField] private float debrisLifetime = 10f;

    [Header("Temporary Testing")]
    [SerializeField] private bool enableTestKey = false;
    [SerializeField] private float testDamage = 25f;

    //[Header("Main Object Hit Reaction")]
   // [SerializeField] private Rigidbody objectRigidbody;
    //[SerializeField] private float hitForce = 0.7f;
   // [SerializeField] private float hitTorque = 0.5f;

 

    private bool damaged;
    private bool destroyed;

    private Rigidbody[] damagePieceBodies;
    private Rigidbody[] rubbleBodies;

    private Collider[] originalColliders;


    private void Awake()
    {
        if (strongholdObject == null)
        {
            strongholdObject =
                GetComponent<StrongholdObject>();
        }

        if (strongholdObject == null)
        {
            strongholdObject =
                GetComponentInParent<StrongholdObject>();
        }

        if (objectRigidbody == null)
        {
            objectRigidbody =
                GetComponent<Rigidbody>();
        }

        // Collect original structural colliders.
        Collider[] allColliders =
            GetComponentsInChildren<Collider>(true);

        System.Collections.Generic.List<Collider> validColliders =
            new System.Collections.Generic.List<Collider>();

        foreach (Collider col in allColliders)
        {
            if (col == null)
                continue;

            // Do not include physics debris colliders.
            if (damagePieces != null &&
                col.transform.IsChildOf(damagePieces.transform))
                continue;

            if (destroyedRubble != null &&
                col.transform.IsChildOf(destroyedRubble.transform))
                continue;

            validColliders.Add(col);
        }

        originalColliders = validColliders.ToArray();

        // Keep the main structure stationary.
        if (objectRigidbody != null)
        {
            objectRigidbody.linearVelocity = Vector3.zero;
            objectRigidbody.angularVelocity = Vector3.zero;
            objectRigidbody.isKinematic = true;
        }

        // Find the little pieces that fly
        // when the object becomes damaged.
        if (damagePieces != null)
        {
            damagePieceBodies =
                damagePieces.GetComponentsInChildren<Rigidbody>(true);

            FreezePieces(damagePieceBodies);
        }

        // Find the final rubble pieces.
        if (destroyedRubble != null)
        {
            rubbleBodies =
                destroyedRubble.GetComponentsInChildren<Rigidbody>(true);

            FreezePieces(rubbleBodies);
        }


        // Starting visuals.
        if (normalVisual != null)
            normalVisual.SetActive(true); //MIGHT NEED TO CHANGE TO FALSE

        if (damagedState != null)
            damagedState.SetActive(false);

        if (damagedVisual != null)
            damagedVisual.SetActive(false);

        if (damagePieces != null)
            damagePieces.SetActive(false);

        if (destroyedRubble != null)
            destroyedRubble.SetActive(false);

        damaged = false;
        destroyed = false;
    }

    private void Update()
    {
        // TEMPORARY TEST ONLY.
        // Ignore H while onboarding has paused the game.
        if (!enableTestKey ||
             Time.timeScale <= 0f ||
             strongholdObject == null)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            Vector3 hitPoint =
                transform.position + Vector3.up * 0.5f;

            strongholdObject.TakeDamage(
                testDamage,
                DamageSource.ZombieAttack,
                hitPoint,
                transform.forward
            );
        }

       // if (Input.GetKeyDown(KeyCode.H))
       // {
          //  Vector3 hitPoint =
          //      transform.position +
          //      Vector3.up * 0.5f;

           // Vector3 hitDirection =
           //     (
           //         transform.right +
            //        transform.forward * 0.2f
             //   ).normalized;

           // strongholdObject.TakeDamage(
           //     testDamage,
           //     DamageSource.ZombieAttack,
           //     hitPoint,
           //     hitDirection
           // );
        //}
    }

    // Called by StrongholdObject whenever
    // this object actually loses health.
    public void HandleDamage(
        float currentHealth,
        float maxHealth,
        DamageSource source,
        Vector3 hitPoint,
        Vector3 hitDirection)
    {
        if (destroyed)
            return;

        // Extra safety.
        if (source == DamageSource.ZombieBomb)
            return;

       // ApplyHitMovement(
          //  hitPoint,
           // hitDirection
       // );

        float healthPercent =
            currentHealth / maxHealth;

        if (!damaged &&
            healthPercent <= damagedHealthPercent)
        {
            EnterDamagedState();
        }

        if (currentHealth <= 0f)
        {
            EnterDestroyedState();
        }
    }

    //Commented out o=for diagnostic testing 13November
  //  private void ApplyHitMovement(
    //    Vector3 hitPoint,
     //   Vector3 hitDirection)
   //{
      //  if (objectRigidbody == null)
       //     return;

       // Vector3 direction = hitDirection;

       // if (direction.sqrMagnitude < 0.001f)
       // {
       //     direction = transform.forward;
       // }

        //objectRigidbody.AddForceAtPosition(
         //   direction.normalized * hitForce,
         //   hitPoint,
          //  ForceMode.Impulse
        //);

       // Vector3 torque =
       //     new Vector3(
        //        Random.Range(-1f, 1f),
        //        Random.Range(-0.2f, 0.2f),
        //        Random.Range(-1f, 1f)
        //    );

        //objectRigidbody.AddTorque(
        //    torque * hitTorque,
        //    ForceMode.Impulse
       // );
   // }

    private void EnterDamagedState()
    {
        damaged = true;

        if (normalVisual != null)
        {
            normalVisual.SetActive(false);
        }

        if (damagedState != null)
        {
            damagedState.SetActive(true);
        }

        if (damagedVisual != null)
        {
            damagedVisual.SetActive(true);
        }

        if (damagePieces != null)
        {
            damagePieces.SetActive(true);

            ReleasePieces(
                damagePieceBodies,
                damagePieces.transform.position,
                damagePieceForce,
                damagePieceUpwardForce,
                damagePieceTorque
            );
        }

        Debug.Log(
            gameObject.name +
            " entered damaged state."
        );
    }

    private void EnterDestroyedState()
    {
        if (destroyed)
            return;

        destroyed = true;

        if (normalVisual != null)
        {
            normalVisual.SetActive(false);
        }

        if (damagedState != null)
            damagedState.SetActive(false);

        if (damagedVisual != null)
        {
            damagedVisual.SetActive(false);
        }

        // Disable original structural colliders.
        foreach (Collider col in originalColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        if (objectRigidbody != null)
        {
            objectRigidbody.linearVelocity =
                Vector3.zero;

            objectRigidbody.angularVelocity =
                Vector3.zero;

            objectRigidbody.isKinematic = true;
        }

        if (destroyedRubble != null)
        {
            destroyedRubble.SetActive(true);

            ReleasePieces(
                rubbleBodies,
                destroyedRubble.transform.position,
                rubbleForce,
                rubbleUpwardForce,
                rubbleTorque
            );
        }

        Debug.Log(
            gameObject.name +
            " entered destroyed state."
        );
    }

    private void FreezePieces(
        Rigidbody[] pieces)
    {
        if (pieces == null)
            return;

        foreach (Rigidbody piece in pieces)
        {
            if (piece == null)
                continue;

            piece.linearVelocity =
                Vector3.zero;

            piece.angularVelocity =
                Vector3.zero;

            piece.useGravity = false;
            piece.isKinematic = true;
        }
    }

    private void ReleasePieces(
        Rigidbody[] pieces,
        Vector3 centre,
        float force,
        float upwardForce,
        float torque)
    {
        if (pieces == null)
            return;

        int debrisLayer =
           LayerMask.NameToLayer(debrisLayerName);


        foreach (Rigidbody piece in pieces)
        {
            if (piece == null)
                continue;

            // Separate the broken piece
            // from the original object.
            piece.transform.SetParent(null, true);

            // Put debris on its dedicated layer.
            if (debrisLayer != -1)
            {
                SetLayerRecursively(
                    piece.gameObject,
                    debrisLayer
                );
            }

            piece.isKinematic = false;
            piece.useGravity = true;

            piece.linearVelocity =
                Vector3.zero;

            piece.angularVelocity =
                Vector3.zero;

            piece.WakeUp();

            // Calculate an outward direction.
            Vector3 direction =
                piece.worldCenterOfMass - centre;

            if (direction.sqrMagnitude < 0.001f)
                direction = Random.insideUnitSphere;

            direction.Normalize();

            Vector3 finalForce =
                direction * force +
                Vector3.up * upwardForce;

            piece.AddForce(
                finalForce,
                ForceMode.Impulse
            );

            piece.AddTorque(
                Random.insideUnitSphere * torque,
                ForceMode.Impulse
            );

            // Remove debris after a short period.
            Destroy(piece.gameObject, debrisLifetime);
        }

        // Small controlled movement.
        //piece.linearVelocity =
         //       new Vector3(
         //           Random.Range(-0.25f, 0.25f),
          //          Random.Range(0.4f, 0.7f),
          //          Random.Range(-0.25f, 0.25f)
          //      );

           // piece.angularVelocity =
            //    Random.insideUnitSphere * 0.4f;
        //}
    }

    private void SetLayerRecursively(
        GameObject obj,
        int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(
                child.gameObject,
                layer
            );
        }
    }

    public float GetHealth()
    {
        if (strongholdObject == null)
            return 0f;

        return strongholdObject.CurrentHealth;
    }
}