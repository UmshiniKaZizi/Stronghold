
using System.Collections.Generic;
using UnityEngine;

public class Stronghold : MonoBehaviour
{
    [Header("Force Field")]
    public float maxForceField = 100f;
    public float currentForceField = 100f;

    [Header("Stronghold Objects")]
    [SerializeField] private StrongholdObject[] destructibleObjects;

    [Header("Enemy Reassignment")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Associated Player")]
[SerializeField] private PlayerHealth associatedPlayer;

    private readonly HashSet<StrongholdObject> destroyedObjects =
        new HashSet<StrongholdObject>();

    private bool isDestroyed;

    public bool IsBreached => currentForceField <= 0f;
    public bool IsDestroyed => isDestroyed;

    public int TotalObjects
    {
        get
        {
            int count = 0;

            if (destructibleObjects != null)
            {
                foreach (StrongholdObject obj in destructibleObjects)
                {
                    if (obj != null)
                        count++;
                }
            }

            return count;
        }
    }

    public int DestroyedObjects => destroyedObjects.Count;

    private void Awake()
    {
        currentForceField = maxForceField;
        isDestroyed = false;
        destroyedObjects.Clear();

        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (destructibleObjects == null)
            return;

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj != null)
                obj.SetStronghold(this);
        }
    }

    // ==========================================
    // FORCE FIELD
    // ==========================================

    public void DamageForceField(float damage)
    {
        if (isDestroyed || IsBreached || damage <= 0f)
            return;

        currentForceField = Mathf.Max(
            currentForceField - damage,
            0f
        );

        Debug.Log(
            "FORCE FIELD DAMAGE | " + name +
            " | " + currentForceField + "/" + maxForceField
        );

        if (IsBreached)
            Debug.Log("FORCE FIELD BREACHED | " + name);
    }

    public void RestoreForceField(float amount)
    {
        if (isDestroyed || amount <= 0f)
            return;

        float previousValue = currentForceField;

        currentForceField = Mathf.Min(
            currentForceField + amount,
            maxForceField
        );

        Debug.Log(
            "FORCE FIELD RESTORED | " + name +
            " | " + previousValue + " -> " +
            currentForceField + "/" + maxForceField
        );
    }

    // ==========================================
    // STRONGHOLD OBJECTS
    // ==========================================

    public StrongholdObject GetClosestAvailableObject(Vector3 position)
    {
        if (isDestroyed || destructibleObjects == null)
            return null;

        StrongholdObject closestObject = null;
        float closestDistance = Mathf.Infinity;

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj == null || obj.IsDestroyed)
                continue;

            float distance = Vector3.Distance(
                position,
                obj.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = obj;
            }
        }

        return closestObject;
    }

    public void RegisterObjectDestroyed(StrongholdObject obj)
    {
        if (isDestroyed || obj == null)
            return;

        if (!destroyedObjects.Add(obj))
            return;

        Debug.Log(
            "STRONGHOLD OBJECT DESTROYED | " + name +
            " | " + destroyedObjects.Count + "/" + TotalObjects
        );

        if (TotalObjects > 0 &&
            destroyedObjects.Count >= TotalObjects)
        {
            DestroyStronghold("All stronghold objects destroyed");
        }
    }

    // ==========================================
    // STRONGHOLD DESTRUCTION
    // ==========================================

    public void DestroyStronghold(string reason)
{
    if (isDestroyed)
        return;

    isDestroyed = true;

    Debug.Log(
        "STRONGHOLD DESTROYED | " + name +
        " | Reason: " + reason
    );

    // Kill the player associated with this stronghold.
    if (associatedPlayer != null && !associatedPlayer.IsDead)
    {
        associatedPlayer.DieFromStrongholdDestruction();
    }

    // Reassign enemies to another surviving stronghold.
    if (enemySpawner == null)
        enemySpawner = FindFirstObjectByType<EnemySpawner>();

    if (enemySpawner != null)
    {
        enemySpawner.ReassignEnemiesFrom(this);
    }
    else
    {
        Debug.LogWarning(
            name + ": No EnemySpawner found for reassignment."
        );
    }
}
}
