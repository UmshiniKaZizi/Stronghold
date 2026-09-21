using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Gun")]
    [SerializeField] private Gun gun;

    private bool isControlled;

    private void Awake()
    {
        if (gun == null)
        {
            gun = GetComponentInChildren<Gun>();
        }

        if (gun == null)
        {
            Debug.LogWarning(
                gameObject.name + " has no Gun assigned!"
            );
        }
    }

    public void OnShoot(InputValue value)
    {
        if (!isControlled)
            return;

        // Only shoot when the button is initially pressed
        if (value.isPressed && gun != null)
        {
            gun.Shoot();

            Debug.Log(
                gameObject.name + " SHOOT"
            );
        }
    }

    public void OnReload(InputValue value)
    {
        if (!isControlled)
            return;

        if (value.isPressed && gun != null)
        {
            Debug.Log(
                gameObject.name + " RELOAD"
            );

            gun.TryReload();
        }
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;

        Debug.Log(
            gameObject.name +
            " SHOOTING CONTROL = " +
            controlled
        );
    }
}