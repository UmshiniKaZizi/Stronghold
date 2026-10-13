
using UnityEngine;

public class SniperScopeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera sniperCamera;
    [SerializeField] private GameObject scopeOverlay;
    [SerializeField] private PlayerLook playerLook;

    [Header("Zoom Settings")]
    [SerializeField] private float scopedFOV = 15f;
    [SerializeField] private float zoomSpeed = 12f;

    [Header("Aiming Sensitivity")]
    [SerializeField] private float scopedSensitivityMultiplier = 0.35f;

    private float normalFOV;
    private bool isControlled;

    public bool IsControlled => isControlled;

    private void Awake()
    {
        if (sniperCamera == null)
            sniperCamera = GetComponentInChildren<Camera>(true);

        if (playerLook == null)
            playerLook = GetComponent<PlayerLook>();

        if (sniperCamera != null)
            normalFOV = sniperCamera.fieldOfView;

        if (scopeOverlay != null)
            scopeOverlay.SetActive(false);
    }

    private void Update()
    {
        if (sniperCamera == null)
            return;

        float targetFOV = isControlled ? scopedFOV : normalFOV;

        sniperCamera.fieldOfView = Mathf.Lerp(
            sniperCamera.fieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;

        if (scopeOverlay != null)
            scopeOverlay.SetActive(controlled);

        if (playerLook != null)
        {
            playerLook.SetScoped(
                controlled,
                scopedSensitivityMultiplier
            );
        }
    }

    private void OnDisable()
    {
        isControlled = false;

        if (scopeOverlay != null)
            scopeOverlay.SetActive(false);

        if (playerLook != null)
            playerLook.SetScoped(false, 1f);
    }
}
