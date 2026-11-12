
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private Gun gun;

    [Header("Artillery")]
    [SerializeField] private ArtilleryBombLauncher bombLauncher;

    [Header("Control")]
    [SerializeField] private bool controlled = true;

    [Header("Aiming")]
    [SerializeField] private PlayerLook playerLook;

    private bool isHoldingShoot;
    private bool bombMode;

    private void Awake()
    {
        if (gun == null)
            gun = GetComponentInChildren<Gun>(true);

        if (bombLauncher == null)
            bombLauncher = GetComponentInChildren<ArtilleryBombLauncher>(true);

        if (playerLook == null)
            playerLook = GetComponent<PlayerLook>();

        isHoldingShoot = false;
        bombMode = false;

        if (gun == null)
        {
            Debug.LogWarning(name + " has no Gun assigned!", this);
        }

        if (bombLauncher == null)
        {
            Debug.Log(name + " has no ArtilleryBombLauncher. Bomb mode unavailable.", this);
        }
    }

    private void Update()
    {
        if (!CanShoot() || !controlled || gun == null)
        {
            StopShooting();
            return;
        }

        // Automatic weapons only operate in normal firing mode.
        if (!bombMode && isHoldingShoot && gun.IsAutomatic())
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

        if (!CanShoot() || !controlled)
            return;

        // Bomb mode: launch one bomb per press.
        if (bombMode)
        {
            if (bombLauncher != null)
                bombLauncher.TryFire();

            return;
        }

        // Normal weapon mode.
        if (gun == null)
            return;

        isHoldingShoot = true;

        gun.SetShooting(true);
        gun.Shoot();

        Debug.Log(name + " | NORMAL SHOOT PRESSED");
    }

    // SHOOT RELEASED
    public void OnShootRelease(InputValue value)
    {
        StopShooting();
    }

    // SWITCH BETWEEN NORMAL AND BOMB MODE
    public void OnSwitchWeapon(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (!CanShoot() || !controlled)
            return;

        if (bombLauncher == null)
        {
            Debug.LogWarning(
                name + " cannot switch modes: no ArtilleryBombLauncher assigned.",
                this
            );
            return;
        }

        // Stop any ongoing normal firing before switching.
        StopShooting();

        bombMode = !bombMode;

        Debug.Log(
            name + " | FIRING MODE: " +
            (bombMode ? "ARTILLERY BOMBS" : "NORMAL AMMUNITION")
        );
    }

    // RELOAD
    public void OnReload(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (!CanShoot() || !controlled || gun == null || bombMode)
            return;

        gun.TryReload();
    }

    // AIM DOWN SIGHTS
    public void OnAim(InputValue value)
    {
        if (!controlled || playerLook == null)
            return;

        bool aiming = value.isPressed;

        if (aiming && (!CanShoot() || bombMode))
            return;

        playerLook.SetAiming(aiming);
    }

    // CHARACTER CONTROL
    public void SetControlled(bool isControlled)
    {
        controlled = isControlled;

        if (!controlled)
        {
            StopShooting();

            // Return to normal mode when control is lost.
            bombMode = false;
        }

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

    public bool IsInBombMode()
    {
        return bombMode;
    }

    public bool IsControlled()
    {
        return controlled;
    }
}
