using UnityEngine;

public class Stronghold : MonoBehaviour
{
    [Header("Force Field")]
    public float maxForceField = 100f;
    public float currentForceField = 100f;

    [Header("Damage")]
    public float forceFieldDamage = 10f;

    public bool IsBreached
    {
        get { return currentForceField <= 0f; }
    }

    private void Start()
    {
        currentForceField = maxForceField;

        Debug.Log(
            gameObject.name +
            " INITIAL FORCE FIELD: " +
            currentForceField +
            "/" +
            maxForceField
        );
    }

    public void DamageForceField(float damage)
    {
        if (IsBreached)
            return;

        currentForceField -= damage;
        currentForceField = Mathf.Max(
            currentForceField,
            0f
        );

        Debug.Log(
            "FORCE FIELD DAMAGE | " +
            gameObject.name +
            " | " +
            currentForceField +
            "/" +
            maxForceField
        );

        if (IsBreached)
        {
            OnForceFieldBreached();
        }
    }

    public void RestoreForceField(float amount)
    {
        float oldForceField = currentForceField;

        currentForceField += amount;

        currentForceField = Mathf.Min(
            currentForceField,
            maxForceField
        );

        Debug.Log(
            "FORCE FIELD RESTORED | " +
            gameObject.name +
            " | " +
            oldForceField +
            " → " +
            currentForceField +
            "/" +
            maxForceField
        );
    }

    private void OnForceFieldBreached()
    {
        Debug.Log(
            "FORCE FIELD BREACHED | " +
            gameObject.name
        );
    }
}