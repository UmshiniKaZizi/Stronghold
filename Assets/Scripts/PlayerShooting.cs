
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Gun")]
    [SerializeField] private Gun gun;

    [Header("Player Look")]
    [SerializeField] private PlayerLook playerLook;

    private bool isControlled;
    private bool isHoldingShoot;

    private void Awake()
    {
        if (gun == null)
            gun = GetComponentInChildren<Gun>(true);

        if (playerLook == null)
            playerLook = GetComponentInChildren<PlayerLook>(true);

        if (playerLook == null)
            playerLook = GetComponentInParent<PlayerLook>();

        if (gun == null)
            Debug.LogWarning(
                gameObject.name + " has no Gun assigned!", this);

        if (playerLook == null)
            Debug.LogWarning(
                gameObject.name + " has no PlayerLook assigned!", this);
    }

    private void Update()
    {
        if (!isControlled || !isHoldingShoot || gun == null)
            return;

        if (gun.IsAutomatic())
            gun.Shoot();
    }

    public void OnShoot(InputValue value)
    {
        if (!isControlled || gun == null)
            return;

        isHoldingShoot = value.isPressed;

        Debug.Log("Shoot input: " + value.isPressed);

        if (isHoldingShoot)
        {
            gun.Shoot();
        }
        else
        {
            gun.StopFiring();
        }
    }
    public void OnShootRelease(InputValue value)
{
    if (!value.isPressed)
    {
        isHoldingShoot = false;

        if (gun != null)
            gun.StopFiring();

        Debug.Log("SHOOT RELEASED");
    }
}

    public void OnReload(InputValue value)
    {
        if (!isControlled || gun == null || !value.isPressed)
            return;

        gun.TryReload();
    }

    public void OnAim(InputValue value)
    {
        bool aiming = isControlled && value.isPressed;

        if (playerLook != null)
            playerLook.SetAiming(aiming);
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;

        if (!controlled)
        {
            isHoldingShoot = false;

            if (playerLook != null)
                playerLook.SetAiming(false);

            if (gun != null)
            {
                gun.StopFiring();
                gun.SetControlled(false);
            }
        }
        else
        {
            if (gun != null)
                gun.SetControlled(true);
        }

        Debug.Log(
            gameObject.name +
            " SHOOTING CONTROL = " +
            controlled
        );
    }

    private void OnDisable()
    {
        isHoldingShoot = false;

        if (gun != null)
            gun.StopFiring();
    }
}
