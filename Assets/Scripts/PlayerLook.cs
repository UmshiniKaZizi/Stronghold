
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float mouseSensitivity = 50f;

    [Header("Controller Sensitivity")]
    [SerializeField] private float controllerSensitivity = 2f;

    [Header("Aim Down Sights")]
    [SerializeField] private float aimSensitivityMultiplier = 0.5f;
    [SerializeField] private float aimFOV = 45f;
    [SerializeField] private float aimTransitionSpeed = 10f;

    [Header("Recoil")]
    [SerializeField] private float recoilSnappiness = 18f;
    [SerializeField] private float recoilReturnSpeed = 4f;
    [SerializeField] private float maxRecoil = 15f;

    private Camera playerCamera;
    private float normalFOV;
    private bool isAiming;

    private Vector2 lookInput;
    private float xRotation;

    private float currentRecoil;
    private float targetRecoil;

    private void Awake()
    {
        if (cam != null)
        {
            playerCamera = cam.GetComponent<Camera>();

            if (playerCamera == null)
                playerCamera = cam.GetComponentInChildren<Camera>();
        }

        if (playerCamera != null)
            normalFOV = playerCamera.fieldOfView;
    }

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleRecoil();
        HandleFOV();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    public void SetScoped(bool scoped, float multiplier)
    {
        isAiming = scoped;
        aimSensitivityMultiplier = scoped ? multiplier : 0.5f;
    }

    private void HandleFOV()
    {
        if (playerCamera == null)
            return;

        // SniperScopeController manages the sniper camera FOV.
        if (GetComponent<SniperScopeController>() != null)
            return;

        float targetFOV = isAiming ? aimFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            aimTransitionSpeed * Time.deltaTime
        );
    }

    private void HandleLook()
    {
        float sensitivity = mouseSensitivity;

        if (Gamepad.current != null &&
            Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f)
        {
            sensitivity = controllerSensitivity;
        }

        if (isAiming)
            sensitivity *= aimSensitivityMultiplier;

        float lookX = lookInput.x * sensitivity * Time.deltaTime;
        float lookY = lookInput.y * sensitivity * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cam != null)
        {
            cam.localRotation = Quaternion.Euler(
                Mathf.Clamp(xRotation - currentRecoil, -90f, 90f),
                0f,
                0f
            );
        }

        transform.Rotate(Vector3.up * lookX);
    }

    public void AddRecoil(float amount)
    {
        targetRecoil = Mathf.Clamp(
            targetRecoil + amount,
            0f,
            maxRecoil
        );
    }

    private void HandleRecoil()
    {
        currentRecoil = Mathf.Lerp(
            currentRecoil,
            targetRecoil,
            recoilSnappiness * Time.deltaTime
        );

        targetRecoil = Mathf.MoveTowards(
            targetRecoil,
            0f,
            recoilReturnSpeed * Time.deltaTime
        );
    }
}
