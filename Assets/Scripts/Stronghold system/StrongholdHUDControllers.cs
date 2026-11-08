using UnityEngine;

public class StrongholdHUDControllers : MonoBehaviour
{
    [SerializeField] private Stronghold[] strongholds;
    [SerializeField] private StrongholdHUDItems[] panels;

    private void Start()
    {
        if (strongholds == null || panels == null || strongholds.Length != panels.Length)
        { Debug.LogError("Assign one HUD panel for every stronghold.", this); enabled = false; return; }
        for (int i = 0; i < strongholds.Length; i++)
        {
            if (strongholds[i] == null || panels[i] == null) { Debug.LogError("A stronghold or HUD panel is unassigned.", this); enabled = false; return; }
            strongholds[i].InitializeState();
            panels[i].Bind(strongholds[i]);
        }
    }
}
