using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class StrongholdHUDItems : MonoBehaviour
{
    [Header("Stronghold Information")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI tierLabel;
    [SerializeField] private TextMeshProUGUI objectCount;
    [SerializeField] private TextMeshProUGUI repairProgress;

    [FormerlySerializedAs("breachBanner")]
    [SerializeField] private TextMeshProUGUI forceFieldStatusLabel;
    //[SerializeField] private TextMeshProUGUI breachBanner;

    [Header("Status Visuals")]
    [SerializeField] private Image stateAccent;
    [SerializeField] private Image objectFill;
    [SerializeField] private Image forceFieldFill;

    [Header("Stronghold Notifications")]
    [SerializeField] private StrongholdEventFeed eventFeed;

    private Stronghold boundSystem;

    public void Bind(Stronghold system)
    {
        if (system == null) 
            return;

        if (boundSystem != null)
            boundSystem.Changed -= Refresh;

        boundSystem = system;
        boundSystem.Changed += Refresh;

        if (title != null)
            title.text = system.DisplayName;

        // Connect this panel's notification feed to this Stronghold.
        if (eventFeed != null)
        {
            system.SetEventFeed(eventFeed);
        }
        else
        {
            Debug.LogWarning(
                "No Event Feed assigned to HUD panel for " +
                system.DisplayName,
                this
            );

        }
        Refresh(system);

           // system.Changed += Refresh;
        //boundSystem = system;
        //if (title != null) title.text = system.DisplayName;
        //Refresh(system);
    }

    public void Refresh(Stronghold system)
    {
        if (system == null)
            return;

        RefreshIntegrity(system);
        RefreshForceField(system);
        RefreshRepairProgress(system);
    
    }

    // ==========================================
    // STRONGHOLD INTEGRITY
    // ==========================================

    private void RefreshIntegrity(Stronghold system)
    {
        int total = Mathf.Max(1, system.TotalObjects);

        Color integrityColor;
        string integrityLabel;

        switch (system.Tier)
        {
            case StrongholdTier.Damaged:
                integrityColor = new Color(1f, 0.78f, 0.15f);
                integrityLabel = "DAMAGED";
                break;

            case StrongholdTier.Critical:
                integrityColor = new Color(1f, 0.48f, 0.08f);
                integrityLabel = "CRITICAL";
                break;

            case StrongholdTier.Destroyed:
                integrityColor = new Color(0.9f, 0.12f, 0.16f);
                integrityLabel = "DESTROYED";
                break;

            default:
                integrityColor = new Color(0.1f, 0.48f, 0.95f);
                integrityLabel = "SECURE";
                break;
        }

        if (tierLabel != null)
        {
            tierLabel.text = integrityLabel;
            tierLabel.color = integrityColor;
        }

        if (objectCount != null)
        {
            objectCount.text =
                system.RemainingObjects + " / " +
                system.TotalObjects + " DEFENCE OBJECTS REMAINING";
        }

        if (stateAccent != null)
            stateAccent.color = integrityColor;

        if (objectFill != null)
        {
            objectFill.fillAmount =
                (float)system.RemainingObjects / total;

            objectFill.color = integrityColor;
        }
    }

    // ==========================================
    // FORCE FIELD STATUS
    // ==========================================

    private void RefreshForceField(Stronghold system)
    {
        float maximum = system.MaxForceField;
        float current = system.ForceField;

        float fillAmount = maximum > 0f
            ? Mathf.Clamp01(current / maximum)
            : 0f;

        string status;
        Color statusColor;

        if (system.IsBreached || current <= 0f)
        {
            status = "BREACHED";
            statusColor = new Color(0.9f, 0.12f, 0.16f);
        }
        else if (current < maximum)
        {
            status = "DAMAGED";
            statusColor = new Color(1f, 0.78f, 0.15f);
        }
        else
        {
            status = "STABLE";
            statusColor = new Color(0.1f, 0.75f, 0.45f);
        }

        if (forceFieldStatusLabel != null)
        {
            forceFieldStatusLabel.gameObject.SetActive(true);
            forceFieldStatusLabel.text =
                //"FORCE FIELD: " + 
                status;

            forceFieldStatusLabel.color = statusColor;
        }

        if (forceFieldFill != null)
        {
            forceFieldFill.fillAmount = fillAmount;
            forceFieldFill.color = statusColor;
        }
    }

    // ==========================================
    // REPAIR PROGRESS
    // ==========================================

    private void RefreshRepairProgress(Stronghold system)
    {
        if (repairProgress == null)
            return;

        int requiredKills = system.KillsPerRepair;
        int currentKills = system.RepairKillProgress;

        repairProgress.text =
           // "REPAIR PROGRESS: " +
            currentKills + " / " +
            requiredKills + " KILLS";
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    private void OnDestroy()
    {
        if (boundSystem != null)
            boundSystem.Changed -= Refresh;
    }

   // private void OnDisable()
   // {
   //     if (boundSystem != null)
   //         boundSystem.Changed -= Refresh;
   // }

}
