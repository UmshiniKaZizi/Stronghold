
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Gun")]
    [SerializeField] private Gun gun;

    [Header("Player Look")]
    [SerializeField] private PlayerLook playerLook;

    private bool isControlled;

    private void Awake()
    {
        if (gun == null)
            gun = GetComponentInChildren<Gun>();

        if (playerLook == null)
            playerLook = GetComponentInChildren<PlayerLook>();

        if (playerLook == null)
            playerLook = GetComponentInParent<PlayerLook>();

        if (gun == null)
            Debug.LogWarning(gameObject.name + " has no Gun assigned!");

        if (playerLook == null)
            Debug.LogWarning(gameObject.name + " has no PlayerLook assigned!");
    }

    public void OnShoot(InputValue value)
    {
        if (!isControlled || !value.isPressed)
            return;

        if (gun != null)
            gun.Shoot();
    }

    public void OnReload(InputValue value)
    {
        if (!isControlled || !value.isPressed)
            return;

        if (gun != null)
            gun.TryReload();
    }

    public void OnAim(InputValue value)
    {
        bool aiming = isControlled && value.isPressed;

        if (playerLook != null)
            playerLook.SetAiming(aiming);

        //if (gun != null)
            //gun.SetAiming(aiming);
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;

        if (!isControlled)
        {
            if (playerLook != null)
                playerLook.SetAiming(false);

            //if (gun != null)
                //gun.SetAiming(false);
        }

        Debug.Log(
            gameObject.name +
            " SHOOTING CONTROL = " +
            controlled
        );
    }
}
