
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

    [Header("Object Destruction Thresholds")]
    [Range(0f, 1f)]
    [SerializeField] private float threatenedThreshold = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float criticalThreshold = 0.5f;

    public StrongholdStatus CurrentStatus { get; private set; }
        = StrongholdStatus.Secure;

    private void Awake()
    {
        if (stronghold == null)
            stronghold = GetComponent<Stronghold>();
    }

    private void Update()
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (stronghold == null)
            return;

        if (stronghold.IsDestroyed)
        {
            CurrentStatus = StrongholdStatus.Destroyed;
            return;
        }

        int totalObjects = stronghold.TotalObjects;

        if (totalObjects <= 0)
        {
            CurrentStatus = StrongholdStatus.Secure;
            return;
        }

        float destroyedFraction =
            (float)stronghold.DestroyedObjects / totalObjects;

        if (destroyedFraction >= criticalThreshold)
        {
            CurrentStatus = StrongholdStatus.Critical;
        }
        else if (destroyedFraction >= threatenedThreshold)
        {
            CurrentStatus = StrongholdStatus.Threatened;
        }
        else
        {
            CurrentStatus = StrongholdStatus.Secure;
        }
    }

    public Color GetStatusColor()
    {
        switch (CurrentStatus)
        {
            case StrongholdStatus.Secure:
                return Color.green;

            case StrongholdStatus.Threatened:
                return Color.yellow;

            case StrongholdStatus.Critical:
                return new Color(1f, 0.5f, 0f);

            case StrongholdStatus.Destroyed:
                return Color.red;

            default:
                return Color.white;
        }
    }

    public void MarkDestroyed()
    {
        if (stronghold != null)
            stronghold.DestroyStronghold("Marked destroyed");

        CurrentStatus = StrongholdStatus.Destroyed;
    }
}
