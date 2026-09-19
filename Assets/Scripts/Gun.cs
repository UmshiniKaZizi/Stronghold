using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private int magSize = 20;
    [SerializeField] private float reloadTime = 1f;

    [Header("Shooting")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 100f;
    [SerializeField] private int damage = 25;

   [Header("Muzzle Flash")]
[SerializeField] private ParticleSystem[] muzzleFlashes;

    [Header("Recoil")]
    [SerializeField] private float recoilRotation = 2f;
    [SerializeField] private float recoilKickback = 0.05f;
    [SerializeField] private float recoilSnappiness = 15f;
    [SerializeField] private float recoilReturnSpeed = 10f;

    [Header("Camera Recoil")]
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private float cameraRecoil = 1.5f;

    [Header("Reload Animation")]
    [SerializeField] private Vector3 reloadRotationOffset =
        new Vector3(66f, 50f, 50f);

    private int currentAmmo;
    private bool isReloading;
    private float nextTimeToFire;

    private Quaternion initialRotation;
    private Vector3 initialPosition;

    private Quaternion recoilTargetRotation;
    private Vector3 recoilTargetPosition;

    private void Start()
    {
        currentAmmo = magSize;

        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;

        recoilTargetRotation = initialRotation;
        recoilTargetPosition = initialPosition;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerLook == null)
        {
            playerLook = GetComponentInParent<PlayerLook>();
        }
    }

    private void Update()
    {
        HandleRecoil();
    }

    public void Shoot()
    {
        if (isReloading)
            return;

        if (Time.time < nextTimeToFire)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        nextTimeToFire = Time.time + fireRate;

        currentAmmo--;

        // Hitscan shooting
        FireRaycast();

        // Weapon recoil
        ApplyRecoil();

        // Camera recoil
        if (playerLook != null)
        {
            playerLook.AddRecoil(cameraRecoil);
        }

       if (muzzleFlashes != null)
{
    foreach (ParticleSystem flash in muzzleFlashes)
    {
        if (flash != null)
        {
            flash.Play();
        }
    }
}

        Debug.Log("SHOT FIRED | Ammo: " + currentAmmo);
    }

    private void FireRaycast()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player Camera is not assigned!");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log(
                "HIT: " +
                hit.collider.gameObject.name +
                " | Distance: " +
                hit.distance
            );

            hit.collider.SendMessage(
                "TakeDamage",
                damage,
                SendMessageOptions.DontRequireReceiver
            );
        }
        else
        {
            Debug.Log("SHOT MISSED");
        }
    }

    private void ApplyRecoil()
    {
        recoilTargetRotation *=
            Quaternion.Euler(-recoilRotation, 0f, 0f);

        recoilTargetPosition +=
            new Vector3(0f, 0f, -recoilKickback);
    }

    private void HandleRecoil()
    {
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            recoilTargetRotation,
            recoilSnappiness * Time.deltaTime
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            recoilTargetPosition,
            recoilSnappiness * Time.deltaTime
        );

        recoilTargetRotation = Quaternion.Slerp(
            recoilTargetRotation,
            initialRotation,
            recoilReturnSpeed * Time.deltaTime
        );

        recoilTargetPosition = Vector3.Lerp(
            recoilTargetPosition,
            initialPosition,
            recoilReturnSpeed * Time.deltaTime
        );
    }

    public void TryReload()
    {
        if (isReloading)
            return;

        if (currentAmmo >= magSize)
            return;

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("RELOADING...");

        Quaternion targetRotation =
            initialRotation *
            Quaternion.Euler(reloadRotationOffset);

        float elapsed = 0f;

        // Rotate gun down
        while (elapsed < reloadTime / 2f)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / (reloadTime / 2f);

            transform.localRotation = Quaternion.Slerp(
                initialRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        // Refill magazine
        currentAmmo = magSize;

        elapsed = 0f;

        // Rotate gun back
        while (elapsed < reloadTime / 2f)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / (reloadTime / 2f);

            transform.localRotation = Quaternion.Slerp(
                targetRotation,
                initialRotation,
                t
            );

            yield return null;
        }

        // Reset gun
        transform.localRotation = initialRotation;
        transform.localPosition = initialPosition;

        recoilTargetRotation = initialRotation;
        recoilTargetPosition = initialPosition;

        isReloading = false;

        Debug.Log(
            "RELOAD COMPLETE | Ammo: " + currentAmmo
        );
    }
}