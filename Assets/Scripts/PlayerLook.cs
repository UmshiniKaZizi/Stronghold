using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{   
    public float mouseSensativity = 50f;
    public Transform cam;
    private float xRotation = 0f;
    private Vector2 lookInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     Cursor.lockState = CursorLockMode.Locked;
     Cursor.visible = false;   
    }

    // Update is called once per frame
    void Update()
    {
       HandleMouseLook(); 
    }
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensativity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensativity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation,-90,90);

        cam.localRotation = Quaternion.Euler(xRotation,0f,0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
