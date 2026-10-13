using UnityEngine;

public class StrongholdHUDController : MonoBehaviour
{
   
    [Header("Stronghold State References")]
    [SerializeField] private StrongholdState[] strongholds;

    [Header("HUD Box References")]
    [SerializeField] private StrongholdHUDItem[] hudItems;

    [Header("Update Settings")]
    [SerializeField, Min(0.05f)]
    private float updateInterval = 0.1f;

    private float updateTimer;

    private void Start()
    {
        // Check that each Stronghold has a matching HUD box.
        if (strongholds == null || hudItems == null ||
            strongholds.Length != 3 || hudItems.Length != 3)
        {
            Debug.LogError(
                "StrongholdHUDController needs exactly " +
                "3 Stronghold references and 3 HUD items."
            );

            enabled = false;
            return;
        }

        // Give each box its letter and show the initial state.
        for (int i = 0; i < hudItems.Length; i++)
        {
            if (strongholds[i] == null || hudItems[i] == null)
            {
                Debug.LogError(
                    "A Stronghold or HUD item reference is missing."
                );

                enabled = false;
                return;
            }

            hudItems[i].SetLabel(
                ((char)('A' + i)).ToString()
            );

            hudItems[i].SetStatus(
                strongholds[i].CurrentStatus
            );
        }
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;

        // Refresh the HUD at regular intervals rather than
        // doing unnecessary UI work every frame.
        if (updateTimer < updateInterval)
            return;

        updateTimer = 0f;

        for (int i = 0; i < strongholds.Length; i++)
        {
            if (strongholds[i] == null || hudItems[i] == null)
                continue;

            hudItems[i].SetStatus(
                strongholds[i].CurrentStatus
            );
        }
    }
}

