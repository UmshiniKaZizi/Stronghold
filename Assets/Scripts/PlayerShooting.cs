
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public Gun gun;

    private bool isHoldingShoot = false;

    // ============================================================
    // SHOOT INPUT
    // ============================================================

    public void OnShoot(InputValue value)
    {
        if (value.isPressed)
        {
            isHoldingShoot = true;

            Debug.Log("SHOOT STARTED");
        }
    }

    // ============================================================
    // SHOOT RELEASE
    // ============================================================

    public void OnShootRelease(InputValue value)
    {
        if (!value.isPressed)
        {
            isHoldingShoot = false;

            Debug.Log("SHOOT RELEASED");
        }
    }

    // ============================================================
    // RELOAD INPUT
    // ============================================================

    public void OnReload(InputValue value)
    {
        if (!value.isPressed)
            return;

        Debug.Log("RELOAD INPUT RECEIVED");

        if (gun != null)
        {
            gun.TryReload();
        }
    }

    // ============================================================
    // SHOOTING
    // ============================================================

    private void Update()
    {
        if (isHoldingShoot && gun != null)
        {
            gun.Shoot();
        }
    }
}
