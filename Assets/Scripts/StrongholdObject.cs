
using UnityEngine;

public class StrongholdObject : MonoBehaviour
{
    [Header("Object Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Destruction")]
    [SerializeField] private GameObject destructionEffect;
    [SerializeField] private bool destroyObjectOnDeath = true;

    private float currentHealth;
    private Stronghold stronghold;
    private bool isDestroyed;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetStronghold(Stronghold owner)
    {
        stronghold = owner;
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed || damage <= 0f)
            return;

        currentHealth = Mathf.Max(
            currentHealth - damage,
            0f
        );

        Debug.Log(
            "OBJECT DAMAGED | " + name +
            " | Health: " + currentHealth +
            "/" + maxHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        Debug.Log("OBJECT DESTROYED | " + name);

        if (stronghold != null)
        {
            stronghold.RegisterObjectDestroyed(this);
        }
        else
        {
            Debug.LogWarning(
                name + " has no Stronghold assigned."
            );
        }

        if (destructionEffect != null)
        {
            Instantiate(
                destructionEffect,
                transform.position,
                transform.rotation
            );
        }

        if (destroyObjectOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            // Keep the object visible but disable interaction.
            foreach (Collider objectCollider in
                     GetComponentsInChildren<Collider>())
            {
                objectCollider.enabled = false;
            }

            foreach (Renderer objectRenderer in
                     GetComponentsInChildren<Renderer>())
            {
                objectRenderer.enabled = false;
            }
        }
    }
}
