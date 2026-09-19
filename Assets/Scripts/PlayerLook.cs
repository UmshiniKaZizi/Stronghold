using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 50f;

    [Header("Controller")]
    [SerializeField] private float controllerSensitivity = 2f;

    [Header("Recoil")]
    [SerializeField] private float recoilSnappiness = 15f;
    [SerializeField] private float recoilReturnSpeed = 10f;

    private Vector2 lookInput;

    private float xRotation;

    // Current and target camera recoil
    private float currentRecoil;
    private float targetRecoil;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleRecoil();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void HandleLook()
    {
        float sensitivity;

        if (Gamepad.current != null &&
            Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f)
        {
            sensitivity = controllerSensitivity;
        }
        else
        {
            sensitivity = mouseSensitivity;
        }

        float lookX = lookInput.x * sensitivity * Time.deltaTime;
        float lookY = lookInput.y * sensitivity * Time.deltaTime;

        // Normal player look
        xRotation -= lookY;

        // Clamp normal camera rotation
        xRotation = Mathf.Clamp(
            xRotation,
            -90f,
            90f
        );

        // Apply look + recoil
        cam.localRotation = Quaternion.Euler(
            xRotation - currentRecoil,
            0f,
            0f
        );

        // Horizontal rotation stays on player
        transform.Rotate(
            Vector3.up * lookX
        );
    }

    public void AddRecoil(float amount)
    {
        // Add recoil to the target
        targetRecoil += amount;

        // Prevent recoil from becoming excessive
        targetRecoil = Mathf.Clamp(
            targetRecoil,
            0f,
            5f
        );
    }

    private void HandleRecoil()
    {
        // Move camera toward recoil target
        currentRecoil = Mathf.Lerp(
            currentRecoil,
            targetRecoil,
            recoilSnappiness * Time.deltaTime
        );

        // Return recoil target toward zero
        targetRecoil = Mathf.Lerp(
            targetRecoil,
            0f,
            recoilReturnSpeed * Time.deltaTime
        );
    }
}