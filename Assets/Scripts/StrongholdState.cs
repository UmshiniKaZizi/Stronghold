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
    [Header("Existing Stronghold Script")]
    [SerializeField] private Stronghold stronghold;

    [Header("State Thresholds")]
    [Range(0f, 1f)]
    [SerializeField] private float secureThreshold = 0.70f;

    [Range(0f, 1f)]
    [SerializeField] private float threatenedThreshold = 0.40f;

    // Only a separate destruction system should set this.
    private bool isDestroyed;

    public StrongholdStatus CurrentStatus { get; private set; }

    private void Awake()
    {
        // Find Mdu's existing component if it is not assigned.
        if (stronghold == null)
            stronghold = GetComponent<Stronghold>();

        if (stronghold == null)
        {
            Debug.LogError(
                name + " needs a Stronghold component."
            );
        }
    }

    private void Update()
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (isDestroyed)
        {
            CurrentStatus = StrongholdStatus.Destroyed;
            return;
        }

        if (stronghold == null ||
            stronghold.maxForceField <= 0f)
        {
            return;
        }

        // Calculate the remaining force-field percentage.
        float strengthPercent =
            stronghold.currentForceField /
            stronghold.maxForceField;

        if (strengthPercent > secureThreshold)
        {
            CurrentStatus = StrongholdStatus.Secure;
        }
        else if (strengthPercent > threatenedThreshold)
        {
            CurrentStatus = StrongholdStatus.Threatened;
        }
        else
        {
            // Includes a breached field at 0% strength.
            CurrentStatus = StrongholdStatus.Critical;
        }
    }

    // Call this from a future Stronghold destruction system.
    public void MarkDestroyed()
    {
        isDestroyed = true;
        CurrentStatus = StrongholdStatus.Destroyed;
    }

    // Useful for debugging and future systems.
    public Color GetStatusColor()
    {
        switch (CurrentStatus)
        {
            case StrongholdStatus.Secure:
                return Color.green;

            case StrongholdStatus.Threatened:
                return Color.yellow;

            case StrongholdStatus.Critical:
                return new Color(1f, 0.35f, 0f);

            case StrongholdStatus.Destroyed:
                return Color.red;

            default:
                return Color.white;
        }
    }
  
}
