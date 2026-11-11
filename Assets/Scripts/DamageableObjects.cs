using UnityEngine;

public enum DamageSource
{
    ZombieAttack,
    PlayerBullet,
    ZombieBomb
}

public class DamageableObject : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Object Visuals")]
    [SerializeField] private GameObject normalVisual;
    [SerializeField] private GameObject damagedState;
    [SerializeField] private GameObject damagedVisual;
    [SerializeField] private GameObject damagePieces;

    [Header("Destroyed State")]
    [SerializeField] private GameObject destroyedRubble;

    [Header("Damage Threshold")]
    [Range(0f, 1f)]
    [SerializeField] private float damagedHealthPercent = 0.6f;

    [Header("Main Object Hit Reaction")]
    [SerializeField] private Rigidbody objectRigidbody;
    [SerializeField] private float hitForce = 0.7f;
    [SerializeField] private float hitTorque = 0.5f;

    [Header("Damage Piece Physics")]
    [SerializeField] private float damagePieceForce = 2f;
    [SerializeField] private float damagePieceUpwardForce = 0.4f;
    [SerializeField] private float damagePieceTorque = 1.5f;

    [Header("Destroyed Rubble Physics")]
    [SerializeField] private float rubbleForce = 2f;
    [SerializeField] private float rubbleUpwardForce = 0.3f;
    [SerializeField] private float rubbleTorque = 1.5f;

    [Header("Temporary Testing")]
    [SerializeField] private float testDamage = 25f;

    private float currentHealth;

    private bool damaged;
    private bool destroyed;

    private Rigidbody[] damagePieceBodies;
    private Rigidbody[] rubbleBodies;

    private void Awake()
    {
        // Find the little wood pieces used
        // when the pallet becomes damaged.
        if (damagePieces != null)
        {
            damagePieceBodies =
                damagePieces.GetComponentsInChildren<Rigidbody>(true);

            FreezePieces(damagePieceBodies);
        }

        // Find all individual pieces in the
        // final destroyed rubble.
        if (destroyedRubble != null)
        {
            rubbleBodies =
                destroyedRubble.GetComponentsInChildren<Rigidbody>(true);

            FreezePieces(rubbleBodies);
        }

        // Hide damaged and destroyed states
        // before gameplay begins.
        if (damagedState != null)
        {
            damagedState.SetActive(false);
        }

        if (destroyedRubble != null)
        {
            destroyedRubble.SetActive(false);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (normalVisual != null)
        {
            normalVisual.SetActive(true);
        }

        if (objectRigidbody == null)
        {
            objectRigidbody =
                GetComponent<Rigidbody>();
        }

        Debug.Log(
            gameObject.name +
            " started with " +
            currentHealth +
            " HP."
        );
    }

    private void Update()
    {
        // TEMPORARY TEST:
        // Press H to simulate damage.
        if (Input.GetKeyDown(KeyCode.H))
        {
            Vector3 hitPoint =
                transform.position +
                Vector3.up * 0.5f;

            Vector3 hitDirection =
                (
                    transform.right +
                    transform.forward * 0.2f
                ).normalized;

            TakeDamage(
                testDamage,
                DamageSource.ZombieAttack,
                hitPoint,
                hitDirection
            );
        }
    }

    public void TakeDamage(
        float damage,
        DamageSource source,
        Vector3 hitPoint,
        Vector3 hitDirection)
    {
        if (destroyed)
            return;

        // Zombie bomb will not damage these objects.
        if (source == DamageSource.ZombieBomb)
        {
            Debug.Log(
                gameObject.name +
                " ignored Zombie Bomb damage."
            );

            return;
        }

        currentHealth -= damage;

        currentHealth =
            Mathf.Max(
                currentHealth,
                0f
            );

        Debug.Log(
            gameObject.name +
            " took " +
            damage +
            " damage. HP: " +
            currentHealth +
            " / " +
            maxHealth
        );

        // Give the main pallet a little physical reaction
        // every time it gets hit.
        ApplyHitMovement(
            hitPoint,
            hitDirection
        );

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

    private void ApplyHitMovement(
        Vector3 hitPoint,
        Vector3 hitDirection)
    {
        if (objectRigidbody == null)
            return;

        objectRigidbody.AddForceAtPosition(
            hitDirection.normalized *
            hitForce,
            hitPoint,
            ForceMode.Impulse
        );

        Vector3 torque =
            new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.2f, 0.2f),
                Random.Range(-1f, 1f)
            );

        objectRigidbody.AddTorque(
            torque *
            hitTorque,
            ForceMode.Impulse
        );
    }

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
        destroyed = true;

        if (normalVisual != null)
        {
            normalVisual.SetActive(false);
        }

        // Hide only the cracked pallet.
        // We leave DamagePieces alone so pieces
        // that already flew off don't disappear.
        if (damagedVisual != null)
        {
            damagedVisual.SetActive(false);
        }

        // Stop the main pallet body.
        if (objectRigidbody != null)
        {
            objectRigidbody.linearVelocity =
                Vector3.zero;

            objectRigidbody.angularVelocity =
                Vector3.zero;

            objectRigidbody.isKinematic = true;
        }

        // Reveal and release the final rubble.
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
            " was destroyed."
        );

        // Stop checking H after destruction.
        enabled = false;
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

        // Stop the wooden pieces from colliding
        // with each other.
        for (int i = 0; i < pieces.Length; i++)
        {
            Collider colliderA =
                pieces[i] != null
                    ? pieces[i].GetComponent<Collider>()
                    : null;

            if (colliderA == null)
                continue;

            for (int j = i + 1; j < pieces.Length; j++)
            {
                Collider colliderB =
                    pieces[j] != null
                        ? pieces[j].GetComponent<Collider>()
                        : null;

                if (colliderB != null)
                {
                    Physics.IgnoreCollision(
                        colliderA,
                        colliderB,
                        true
                    );
                }
            }
        }

        foreach (Rigidbody piece in pieces)
        {
            if (piece == null)
                continue;

            piece.isKinematic = false;
            piece.useGravity = true;

            piece.linearVelocity = Vector3.zero;
            piece.angularVelocity = Vector3.zero;

            // Very small controlled movement.
            piece.linearVelocity =
                new Vector3(
                    Random.Range(-0.15f, 0.15f),
                    Random.Range(0.2f, 0.4f),
                    Random.Range(-0.15f, 0.15f)
                );

            // Very small rotation.
            piece.angularVelocity =
                Random.insideUnitSphere * 0.3f;
        }
    }
    public float GetHealth()
    {
        return currentHealth;
    }
}