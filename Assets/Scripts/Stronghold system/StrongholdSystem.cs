using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable] public class StrongholdMessageEvent: UnityEvent<string> { }


public class StrongholdSystem : MonoBehaviour
{
    [SerializeField] private string displayName = "Stronghold A";
    [SerializeField] private StrongholdObject[] objects;
    [SerializeField, Min(1f)] private float maxForceField = 100f;
    [SerializeField, Min(1)] private int killsPerRepair = 10;
    [SerializeField] private int criticalObjectCount = 5;
    [SerializeField] private GameObject associatedPlayer;
    [SerializeField] private StrongholdEventFeed eventFeed;
    [SerializeField] private UnityEvent defeatPlayer;
    [SerializeField] private UnityEvent reassignEnemies;

    public event Action<StrongholdSystem> Changed;
    public string DisplayName => displayName;
    public float ForceField { get; private set; }
    public float MaxForceField => maxForceField;
    public int RepairKillProgress { get; private set; }
    public int TotalObjects => objects == null ? 0 : objects.Length;
    public int RemainingObjects
    {
        get { int count = 0; if (objects != null) foreach (var obj in objects) if (obj != null && !obj.IsDestroyed) count++; return count; }
    }

    public bool IsBreached => ForceField <= 0f;
    public StrongholdTier Tier => RemainingObjects == 0 ? StrongholdTier.Destroyed :
        RemainingObjects <= criticalObjectCount ? StrongholdTier.Critical :
        RemainingObjects < TotalObjects ? StrongholdTier.Damaged : StrongholdTier.Secure;

    private void Awake()
    {
        ForceField = maxForceField;
        if (objects == null || objects.Length == 0) objects = GetComponentsInChildren<StrongholdObject>(true);
        foreach (var obj in objects) if (obj != null)
        {
                obj.Destroyed.AddListener(OnObjectDestroyed);
                obj.Damaged.AddListener(OnObjectDamaged);
        }
    }

    public void DamageForceField(float amount)
    {
        if (amount <= 0f || IsBreached || Tier == StrongholdTier.Destroyed) return;
        bool wasBreached = IsBreached;
        ForceField = Mathf.Max(0f, ForceField - amount);
        if (!wasBreached && IsBreached) Publish(displayName + " perimeter breached");
        else Changed?.Invoke(this);
    }

    public void RestoreForceField(float amount)
    {
        if (amount <= 0f || Tier == StrongholdTier.Destroyed) return;
        bool wasBreached = IsBreached;
        ForceField = Mathf.Min(maxForceField, ForceField + amount);
        if (wasBreached && !IsBreached) Publish(displayName + " perimeter restored");
        else Changed?.Invoke(this);
    }

    public void RegisterPlayerKill(GameObject player)
    {
        if (player == null || player != associatedPlayer || Tier == StrongholdTier.Destroyed || killsPerRepair < 1) return;
        RepairKillProgress++;
        if (RepairKillProgress >= killsPerRepair)
        {
            RepairKillProgress = 0;
           // foreach (var obj in objects)
               // if (obj != null && obj.RepairToFull()) { Publish(displayName + " restored (+1 object)"); return; }
        }
        Publish(displayName + " repair progress " + RepairKillProgress + "/" + killsPerRepair);
    }

    public StrongholdObject FindObject(Vector3 position)
    {
        StrongholdObject best = null; float distance = float.PositiveInfinity;
        foreach (var obj in objects) if (obj != null && !obj.IsDestroyed)
            { float d = (obj.transform.position - position).sqrMagnitude; if (d < distance) { distance = d; best = obj; } }
        return best;
    }

    private void OnObjectDestroyed(StrongholdObject obj)
    {
        if (Tier == StrongholdTier.Destroyed)
        {
            Publish(displayName + " destroyed");
            defeatPlayer?.Invoke();
            reassignEnemies?.Invoke();
        }
        else Publish(displayName + " object destroyed (" + RemainingObjects + "/" + TotalObjects + " remain)");
    }

    private void OnObjectDamaged(StrongholdObject obj, float damage)
    {
        Publish(displayName + " object hit (-" + Mathf.RoundToInt(damage) + ")");
    }

    private void Publish(string message)
    {
        Changed?.Invoke(this);
        if (eventFeed != null) eventFeed.Show(message);
    }

}
