using UnityEngine;

public enum StrongholdStatus
{
    Secure,
    Threatened,
    Critical,
    Destroyed
}

public class StrongholdState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Stronghold stronghold;
    [SerializeField] private StrongholdAudio strongholdAudio;
    public StrongholdStatus CurrentStatus { get; private set; }
        = StrongholdStatus.Secure;

    private bool hasInitialized;
    private bool previousForceFieldBreached;

    [Header("Object Destruction Thresholds")]
    [Range(0f, 1f)]
    [SerializeField] private float threatenedThreshold = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float criticalThreshold = 0.5f;


    private void Start()
    {
        if (stronghold != null)
        {
            previousForceFieldBreached = stronghold.IsBreached;
        }
    }

    private void Awake()
    {
        if (stronghold == null)
            stronghold = GetComponent<Stronghold>();

        if (strongholdAudio == null)
            strongholdAudio = GetComponent<StrongholdAudio>();
    }

    private void Update()
    {
        UpdateStatus();
        CheckForceField();

    }

    private void UpdateStatus()
    {
        if (stronghold == null)
            return;

        StrongholdStatus newStatus;

        switch (stronghold.Tier)
        {
            case StrongholdTier.Damaged:
                newStatus = StrongholdStatus.Threatened;
                break;

            case StrongholdTier.Critical:
                newStatus = StrongholdStatus.Critical;
                break;

            case StrongholdTier.Destroyed:
                newStatus = StrongholdStatus.Destroyed;
                break;

            default:
                newStatus = StrongholdStatus.Secure;
                break;
        }

        // Initialise without playing sounds just because the scene started.
        if (!hasInitialized)
        {
            CurrentStatus = newStatus;
            previousForceFieldBreached = stronghold.IsBreached;
            hasInitialized = true;
            return;
        }

        if (newStatus == CurrentStatus)
            return;

        CurrentStatus = newStatus;

        if (strongholdAudio != null)
            strongholdAudio.UpdateAlert(CurrentStatus);
    }

    private void CheckForceField()
    {
        if (stronghold == null || !hasInitialized)
            return;

        bool currentlyBreached = stronghold.IsBreached;

        // Play only when the force field changes from intact to breached.
        if (currentlyBreached && !previousForceFieldBreached)
        {
            if (strongholdAudio != null)
                strongholdAudio.PlayForceFieldBreached();
        }

       // if (!currentlyBreached && strongholdAudio != null)
       // {
        //    strongholdAudio.ResetForceFieldBreachAlert();
        //}

        previousForceFieldBreached = currentlyBreached;
    }

   
    public Color GetStatusColor()
    {


        switch (CurrentStatus)
        {
            case StrongholdStatus.Secure:
                return new Color32(54, 169, 255, 255);

            case StrongholdStatus.Threatened:
                return new Color32(255, 199, 38, 255);

            case StrongholdStatus.Critical:
                return new Color32(255, 122, 20, 255);

            case StrongholdStatus.Destroyed:
                return new Color32(230, 31, 41, 255);

            default:
                return new Color32(54, 169, 255, 255);
        }
    }
        
        
    public void MarkDestroyed()
    {
        if (stronghold != null)
            stronghold.DestroyStronghold("Marked destroyed");

        CurrentStatus = StrongholdStatus.Destroyed;
    }
}
