
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private Gun gun;

    [Header("Control")]
    [SerializeField] private bool controlled = true;

    private bool isHoldingShoot;

    private void Awake()
    {
        if (gun == null)
            gun = GetComponentInChildren<Gun>(true);

        if (gun == null)
        {
            Debug.LogWarning(
                name + " has no Gun assigned!",
                this
            );
        }

        isHoldingShoot = false;
    }

    private void Update()
    {
        if (!CanShoot() || !controlled || gun == null)
        {
            StopShooting();
            return;
        }

        // Continue firing while the shoot button is held
        // if the weapon is automatic.
        if (isHoldingShoot && gun.IsAutomatic())
        {
            gun.Shoot();
        }
    }

    // Called by PlayerInput when the Shoot action is triggered.
    public void OnShoot(InputValue value)
    {
        // Always handle release, even if the character
        // is no longer controlled.
        if (!value.isPressed)
        {
            isHoldingShoot = false;

            if (gun != null)
                gun.StopFiring();

            Debug.Log(name + " | SHOOT RELEASED");
            return;
        }

        if (!CanShoot() || !controlled || gun == null)
        {
            Debug.Log(
                name + " | SHOOT BLOCKED | Started: " +
                (GameManager.Instance != null &&
                 GameManager.Instance.HasGameStarted) +
                " | Controlled: " + controlled +
                " | Gun assigned: " + (gun != null)
            );

            return;
        }

        isHoldingShoot = true;

        Debug.Log(name + " | SHOOT PRESSED");

        // Fire immediately on press.
        gun.Shoot();
    }

    // Called by PlayerInput for the Reload action.
    public void OnReload(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (!CanShoot() || !controlled || gun == null)
        {
            Debug.Log(name + " | RELOAD BLOCKED");
            return;
        }

        gun.TryReload();
    }

    // Preserve compatibility with an Input Action named Aim.
    // Add your previous aiming implementation here if needed.
    public void OnAim(InputValue value)
    {
        if (!CanShoot() || !controlled)
            return;
    }

    // Called by CharacterSwap when changing characters.
    public void SetControlled(bool isControlled)
    {
        controlled = isControlled;
        isHoldingShoot = false;

        if (!controlled && gun != null)
        {
            gun.StopFiring();
        }

        Debug.Log(
            name + " SHOOTING CONTROL = " + controlled
        );
    }

    private bool CanShoot()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.HasGameStarted &&
               !GameManager.Instance.IsGameOver;
    }

    private void StopShooting()
    {
        if (!isHoldingShoot)
            return;

        isHoldingShoot = false;

        if (gun != null)
            gun.StopFiring();
    }
}
