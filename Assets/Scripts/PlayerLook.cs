using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cam;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 10f;
    [SerializeField] private float controllerSensitivity = 100f;

    [Header("Vertical Look")]
    [SerializeField] private float minLookAngle = -90f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("Controller")]
    [SerializeField] private float controllerDeadzone = 0.1f;

    private Vector2 lookInput;
    private float xRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void HandleLook()
    {
        if (lookInput == Vector2.zero)
            return;

        // Detect whether this is likely controller input.
        bool controllerInput = lookInput.magnitude <= 1.0f;

        float sensitivity = controllerInput
            ? controllerSensitivity
            : mouseSensitivity;

        float lookX = lookInput.x * sensitivity * Time.deltaTime;
        float lookY = lookInput.y * sensitivity * Time.deltaTime;

        xRotation -= lookY;

        xRotation = Mathf.Clamp(
            xRotation,
            minLookAngle,
            maxLookAngle
        );

        cam.localRotation = Quaternion.Euler(
            xRotation,
            0f,
            0f
        );

        transform.Rotate(
            Vector3.up * lookX
        );
    }
}