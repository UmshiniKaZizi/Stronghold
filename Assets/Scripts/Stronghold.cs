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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DamageForceField(forceFieldDamage);
        }
    }

    public void DamageForceField(float damage)
    {
        if (IsBreached)
            return;

        currentForceField -= damage;
        currentForceField = Mathf.Max(currentForceField, 0f);

        Debug.Log(
            gameObject.name +
            " Force Field: " +
            currentForceField +
            "/" +
            maxForceField
        );

        if (IsBreached)
        {
            OnForceFieldBreached();
        }
    }

    private void OnForceFieldBreached()
    {
        Debug.Log(gameObject.name + " FORCE FIELD BREACHED!");
    }
}