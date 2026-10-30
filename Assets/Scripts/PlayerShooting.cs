
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private Gun gun;

    [Header("Control")]
    [SerializeField] private bool controlled = true;

    [Header("Aiming")]
    [SerializeField] private PlayerLook playerLook;

    private bool isHoldingShoot;

    private void Awake()
    {
        if (gun == null)
            gun = GetComponentInChildren<Gun>(true);

        if (playerLook == null)
            playerLook = GetComponent<PlayerLook>();

        isHoldingShoot = false;

        if (gun == null)
        {
            Debug.LogWarning(
                name + " has no Gun assigned!",
                this
            );
        }
    }

    private void Update()
    {
        if (!CanShoot() || !controlled || gun == null)
        {
            StopShooting();
            return;
        }

        // Automatic weapons continue firing while held.
        if (isHoldingShoot && gun.IsAutomatic())
        {
            gun.SetShooting(true);
            gun.Shoot();
        }
    }

    // SHOOT PRESSED
    public void OnShoot(InputValue value)
    {
        if (!value.isPressed)
        {
            StopShooting();
            return;
        }

        if (!CanShoot() || !controlled || gun == null)
            return;

        isHoldingShoot = true;

        gun.SetShooting(true);

        // Fire immediately on press.
        gun.Shoot();

        Debug.Log(name + " | SHOOT PRESSED");
    }

    // SHOOT RELEASED
    // Connected to the separate ShootRelease Input Action.
    public void OnShootRelease(InputValue value)
    {
        StopShooting();

        Debug.Log(name + " | SHOOT RELEASED");
    }

    // RELOAD
    public void OnReload(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (!CanShoot() || !controlled || gun == null)
            return;

        gun.TryReload();
    }

    // AIM DOWN SIGHTS
    public void OnAim(InputValue value)
    {
        if (!controlled || playerLook == null)
            return;

        bool aiming = value.isPressed;

        if (aiming && !CanShoot())
            return;

        playerLook.SetAiming(aiming);
    }

    // CHARACTER CONTROL
    public void SetControlled(bool isControlled)
    {
        controlled = isControlled;

        if (!controlled)
            StopShooting();

        if (gun != null)
            gun.SetControlled(isControlled);

        if (playerLook != null)
            playerLook.SetAiming(false);

        Debug.Log(
            name + " SHOOTING CONTROL = " + controlled
        );
    }

    // STOP SHOOTING
    private void StopShooting()
    {
        isHoldingShoot = false;

        if (gun != null)
        {
            gun.SetShooting(false);
            gun.StopFiring();
        }
    }

    // GAME STATE CHECK
    private bool CanShoot()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.HasGameStarted &&
               !GameManager.Instance.IsGameOver;
    }
}
