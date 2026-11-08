using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum StrongholdTier 
{ 
    Secure, 
    Damaged, 
    Critical, 
    Destroyed
}

public class Stronghold : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string displayName = "Stronghold";

    [Header("Force Field")]
    public float maxForceField = 100f;
    public float currentForceField = 100f;

    [Header("Stronghold Objects")]
    [SerializeField] private StrongholdObject[] destructibleObjects;
    [SerializeField, Min(1)] private int criticalObjectCount = 5;

    [Header("Repair")]
    [SerializeField, Min(1)] private int killsPerRepair = 10;
    [SerializeField] private PlayerHealth associatedPlayer;

    [Header("Enemy Reassignment")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Feedback")]
    [SerializeField] private StrongholdEventFeed eventFeed;
    [SerializeField] private UnityEvent onCriticalEntered;
    [SerializeField] private UnityEvent onCriticalExited;

    private readonly HashSet<StrongholdObject> destroyedObjects = 
        new HashSet<StrongholdObject>();

    private int repairKillProgress;
    private bool isDestroyed;
    private bool initialized;
    private bool stateInitialized;

    public event Action<Stronghold> Changed;
    public string DisplayName => 
        string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public float MaxForceField => maxForceField;
    public float ForceField => currentForceField;
    public int RepairKillProgress => repairKillProgress;
    public int KillsPerRepair => Mathf.Max(1, killsPerRepair);
    public bool IsBreached => currentForceField <= 0f;
    public bool IsDestroyed => isDestroyed;
    public int TotalObjects => CountAssignedObjects();
    public int DestroyedObjects => destroyedObjects.Count;
    public int RemainingObjects => 
        Mathf.Max(0, TotalObjects - DestroyedObjects);

    public StrongholdTier Tier
    {
        get
        {
            if (IsDestroyed || RemainingObjects == 0) 
                return StrongholdTier.Destroyed;

            if (RemainingObjects <= Mathf.Max(1, criticalObjectCount)) 
                return StrongholdTier.Critical;

            if (RemainingObjects < TotalObjects || HasDamagedObjects()) 
                return StrongholdTier.Damaged;

            return StrongholdTier.Secure;
        }
    }

    private bool HasDamagedObjects()
    {
        if (destructibleObjects == null)
            return false;

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj == null || obj.IsDestroyed)
                continue;

            if (obj.CurrentHealth < obj.MaxHealth)
                return true;
        }

        return false;
    }

    //  [Header("Associated Player")]
    //[SerializeField] private PlayerHealth associatedPlayer;

    //  private readonly HashSet<StrongholdObject> destroyedObjects =
    //  new HashSet<StrongholdObject>();

    // private bool isDestroyed;

    // public bool IsBreached => currentForceField <= 0f;
    // public bool IsDestroyed => isDestroyed;

    // public int TotalObjects
    // {
    //    get
    //   {
    //     int count = 0;

    //    if (destructibleObjects != null)
    //   {
    //     foreach (StrongholdObject obj in destructibleObjects)
    //     {
    //         if (obj != null)
    //             count++;
    //     }
    //  }

    //     return count;
    //  }
    // }

    //public int DestroyedObjects => destroyedObjects.Count;

    private void Awake()
    {
        currentForceField = Mathf.Clamp
            (currentForceField, 
            0f, 
            Mathf.Max(1f, maxForceField)
            );

        if (destructibleObjects == null || destructibleObjects.Length == 0)
            destructibleObjects = GetComponentsInChildren<StrongholdObject>(true);
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
       // if (eventFeed == null)
         //   eventFeed = FindFirstObjectByType<StrongholdEventFeed>();

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj == null) 
                continue;
            obj.SetStronghold(this);
            obj.Destroyed.AddListener(OnObjectDestroyed);
            obj.Damaged.AddListener(OnObjectDamaged);
        }

        // currentForceField = maxForceField;
        // isDestroyed = false;
        // destroyedObjects.Clear();

        //if (enemySpawner == null)
        //   enemySpawner = FindFirstObjectByType<EnemySpawner>();

        // if (destructibleObjects == null)
        //    return;

        //foreach (StrongholdObject obj in destructibleObjects)
        // {
        //   if (obj != null)
        //       obj.SetStronghold(this);
        // }
    }

    private void Start()
    {
        InitializeState();
    }
    

    /// <summary>Safe to call from HUD setup after every object's Awake has run.</summary>
    public void InitializeState()
    {
        if (stateInitialized) 
            return;
        destroyedObjects.Clear();

        foreach (StrongholdObject obj in destructibleObjects)
            if (obj != null && obj.IsDestroyed) destroyedObjects.Add(obj);
        isDestroyed = TotalObjects == 0 || destroyedObjects.Count >= TotalObjects;
        initialized = true;
        stateInitialized = true;
    }

    // ==========================================
    // FORCE FIELD
    // ==========================================

    public void DamageForceField(float damage)
    {
        if (isDestroyed || IsBreached || damage <= 0f) return;
        currentForceField = Mathf.Max(0f, currentForceField - damage);
        Publish(IsBreached ? DisplayName + " perimeter breached" : null);
       // if (isDestroyed || IsBreached || damage <= 0f)
         //   return;

       // currentForceField = Mathf.Max(
          //  currentForceField - damage,
          //  0f
        //);

       // Debug.Log(
       //     "FORCE FIELD DAMAGE | " + name +
        //    " | " + currentForceField + "/" + maxForceField
        //);

       // if (IsBreached)
         //   Debug.Log("FORCE FIELD BREACHED | " + name);
    }


    public void RestoreForceField(float amount)
    {
        if (isDestroyed || amount <= 0f)
            return;
        
        bool wasBreached = IsBreached;
        currentForceField = Mathf.Min(
            Mathf.Max(1f, maxForceField), 
            currentForceField + amount
            );
        Publish(wasBreached && !IsBreached ? DisplayName + " perimeter restored" : null);
        
        //float previousValue = currentForceField;

       // currentForceField = Mathf.Min(
        //    currentForceField + amount,
         //   maxForceField
       // );

        //Debug.Log(
        //    "FORCE FIELD RESTORED | " + name +
        //    " | " + previousValue + " -> " +
         //   currentForceField + "/" + maxForceField
       // );
    }

    /// <summary>Call once for a player kill. Only this stronghold's player earns repair progress.</summary>
    public void RegisterPlayerKill(GameObject player)
    {

        // Only the assigned character earns repair progress.
        if (player == null || 
            associatedPlayer == null || 
            player != associatedPlayer.gameObject || 
            isDestroyed)
        {
            return;
        }

        repairKillProgress++;

        int requiredKills = Mathf.Max(1, killsPerRepair);
        int repairsCompleted = 0;

        // Continue repairing while enough kills are banked
        // and there are objects that can be restored.
        while (repairKillProgress >= requiredKills && !isDestroyed)
        {
            StrongholdObject repairTarget = FindRepairTarget();

            // Keep earned progress if there is nothing to repair yet.
            if (repairTarget == null)
                break;

            bool repairSucceeded = repairTarget.RepairToFull();

            if (!repairSucceeded)
                break;

            repairKillProgress -= requiredKills;
            repairsCompleted++;
        }
        
        if (repairsCompleted > 0)
        {
            Publish(
                DisplayName + " repaired " +
                repairsCompleted + " object(s). " +
                "Repair progress: " +
                repairKillProgress + "/" + requiredKills
            );
        }
        else
        {
            Publish(
                DisplayName + " repair progress " +
                repairKillProgress + "/" + requiredKills
            );
        }
    }

       
    //Commented ou on 07 November
        //if (repairKillProgress >= Mathf.Max(1, killsPerRepair))
       // {
          //  StrongholdObject repairTarget = FindDestroyedObject();
          //  if (repairTarget != null)
          //  {
            //    repairKillProgress -= Mathf.Max(1, killsPerRepair);
            //    repairTarget.RepairToFull();
             //   Publish(DisplayName + " restored (+1 object)");
             //   return;
          //  }
       // }
        //Publish(DisplayName + " repair progress " + repairKillProgress + "/" + Mathf.Max(1, killsPerRepair));
    



    // ==========================================
    // STRONGHOLD OBJECTS
    // ==========================================

    public StrongholdObject GetClosestAvailableObject(Vector3 position)
    {
        StrongholdObject closest = null;
        float closestDistance = float.PositiveInfinity;

        if (destructibleObjects == null) 
            return null;

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj == null || obj.IsDestroyed) 
                continue;
            float distance = 
                (obj.transform.position - position).sqrMagnitude;
            if (distance < closestDistance) 
            { 
                closestDistance = distance; 
                closest = obj; 
            }
        }
        return closest;

      //  if (isDestroyed || destructibleObjects == null)
        //    return null;

        //StrongholdObject closestObject = null;
       // float closestDistance = Mathf.Infinity;

       // foreach (StrongholdObject obj in destructibleObjects)
       // {
       //     if (obj == null || obj.IsDestroyed)
        //        continue;

        //    float distance = Vector3.Distance(
          //      position,
          //      obj.transform.position
          //  );

          //  if (distance < closestDistance)
         //   {
         //      closestDistance = distance;
         //       closestObject = obj;
       //     }
       // }

       // return closestObject;
    }

    public void RegisterObjectDestroyed(StrongholdObject obj)
    {
        if (obj == null || isDestroyed || destroyedObjects.Contains(obj)) return;
        StrongholdTier previousTier = Tier;
        destroyedObjects.Add(obj);
        EvaluateTierChange(previousTier);

        if (TotalObjects > 0 && destroyedObjects.Count >= TotalObjects)
        {
            DestroyStronghold("All stronghold objects destroyed");
            return;
        }
        Publish(DisplayName + " object destroyed (" + RemainingObjects + "/" + TotalObjects + " remain)");

       // if (isDestroyed || obj == null)
         //   return;

       // if (!destroyedObjects.Add(obj))
       //     return;

      //  Debug.Log(
      //      "STRONGHOLD OBJECT DESTROYED | " + name +
       //     " | " + destroyedObjects.Count + "/" + TotalObjects
       // );

       // if (TotalObjects > 0 &&
       //     destroyedObjects.Count >= TotalObjects)
       // {
        //    DestroyStronghold("All stronghold objects destroyed");
        //}
    }

    public void NotifyObjectRepaired(
        StrongholdObject obj,
        StrongholdTier previousTier
        )
    {
        if (obj == null || isDestroyed)
            return;

        // Remove it if it was previously destroyed.
        // This also works for partially damaged objects.
        destroyedObjects.Remove(obj);

        EvaluateTierChange(previousTier);

        Publish(DisplayName + " object restored");
    }

    /// <summary>
    /// Selects a repair target.
    /// Priority 1: destroyed objects.
    /// Priority 2: the surviving object with the lowest current health.
    /// Fully healthy objects are ignored.
    /// </summary>
    private StrongholdObject FindRepairTarget()
    {
        if (destructibleObjects == null)
            return null;

        // First priority: restore a destroyed object.
        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj != null && obj.IsDestroyed)
                return obj;
        }

        // Second priority: fully restore the most damaged survivor.
        StrongholdObject mostDamagedObject = null;
        float lowestHealth = float.PositiveInfinity;

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj == null || obj.IsDestroyed)
                continue;

            if (obj.CurrentHealth >= obj.MaxHealth)
                continue;

            if (obj.CurrentHealth < lowestHealth)
            {
                lowestHealth = obj.CurrentHealth;
                mostDamagedObject = obj;
            }
        }

        return mostDamagedObject;
    }

    private int CountAssignedObjects()
    {
        if (destructibleObjects == null)
            return 0;

        int count = 0;

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj != null)
                count++;
        }

        return count;
    }

    // ==========================================
    // STRONGHOLD DESTRUCTION
    // ==========================================

    public void DestroyStronghold(string reason)
    {
      if (isDestroyed)
        return;
        StrongholdTier previousTier = Tier;
        isDestroyed = true;
        if (previousTier == StrongholdTier.Critical) 
            onCriticalExited?.Invoke();
        Publish(DisplayName + " destroyed");

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
            name + ": No EnemySpawner found for reassignment.",
            this
            );
        }

    }

    private void OnObjectDestroyed(StrongholdObject obj)
    {
        RegisterObjectDestroyed(obj);
    }
    

    private void OnObjectDamaged(StrongholdObject obj, float damage)
    {
        Publish(
            DisplayName + " object hit (-" + 
            Mathf.RoundToInt(damage) + ")"
            );
    }

    private void EvaluateTierChange(StrongholdTier previousTier)
    {
        if (!initialized)
            return;

        StrongholdTier currentTier = Tier;

        if (previousTier != StrongholdTier.Critical &&
            currentTier == StrongholdTier.Critical)
        {
            onCriticalEntered?.Invoke();
        }
        else if (previousTier == StrongholdTier.Critical &&
                 currentTier != StrongholdTier.Critical)
        {
            onCriticalExited?.Invoke();
        }
    }

    private void Publish(string message)
    {
        Changed?.Invoke(this);

        if (!string.IsNullOrEmpty(message) && eventFeed != null)
            eventFeed.Show(message);
    }

    private void OnDestroy()
    {
        if (destructibleObjects == null)
            return;

        foreach (StrongholdObject obj in destructibleObjects)
        {
            if (obj == null)
                continue;

            obj.Destroyed.RemoveListener(OnObjectDestroyed);
            obj.Damaged.RemoveListener(OnObjectDamaged);
        }
    }

    public void SetEventFeed(StrongholdEventFeed feed)
    {
        eventFeed = feed;

        if (eventFeed != null)
            eventFeed.SetStrongholdName(DisplayName);
    }

    private StrongholdObject FindDestroyedObject()
    {
        if (destructibleObjects == null) return null;
        foreach (StrongholdObject obj in destructibleObjects)
            if (obj != null && obj.IsDestroyed) return obj;
        return null;
    }

 

}
