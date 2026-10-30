using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public enum WeaponType
    {
        Sniper,
        AssaultRifle,
        MachineGun
    }

    [Header("Weapon Type")]
    [SerializeField] private WeaponType weaponType =
        WeaponType.AssaultRifle;

    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private int magSize = 20;
    [SerializeField] private float reloadTime = 1f;

    [Header("Shooting")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 100f;
    [SerializeField] private int damage = 25;
    [Tooltip("Untick the Player layer so shots ignore your own colliders.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Muzzle Point")]
    [Tooltip("Optional transform at the end of the gun barrel.")]
    [SerializeField] private Transform muzzlePoint;

    [Header("Muzzle Flash")]
    [SerializeField] private ParticleSystem[] muzzleFlashes;

    [Header("Weapon Animation")]
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private string fireAnimationTrigger = "Fire";

    [Header("Camera Recoil")]
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private float cameraRecoil = 1.5f;

    [Header("Machine Gun Ammunition Movement")]
    [SerializeField] private Transform ammoPivot;
    [SerializeField] private Vector3 firingTilt =
        new Vector3(0f, 0f, -8f);
    [SerializeField] private float ammoTiltSpeed = 12f;

    [Header("Bullet Impact")]
    [SerializeField] private GameObject bulletDecalPrefab;
    [SerializeField] private float decalOffset = 0.01f;
    [SerializeField] private float decalLifetime = 10f;

    [Header("Bullet Tracer")]
    [SerializeField] private LineRenderer tracerPrefab;
    [SerializeField] private float tracerDuration = 0.05f;
    [SerializeField] private float tracerWidth = 0.02f;

    [Header("Reload Animation")]
    [SerializeField] private Vector3 reloadRotationOffset =
        new Vector3(66f, 50f, 50f);

    private int currentAmmo;
    private float nextFireTime;

    private bool isReloading;
    private bool isFiring;
    private bool isControlled = true;

    private Quaternion ammoRestRotation;
    private Quaternion originalWeaponRotation;

    private void Awake()
    {
        currentAmmo = magSize;

        originalWeaponRotation = transform.localRotation;

        if (ammoPivot != null)
            ammoRestRotation = ammoPivot.localRotation;
    }

    private void Update()
    {
        HandleAmmoTilt();
    }

    private void HandleAmmoTilt()
    {
        if (weaponType != WeaponType.MachineGun ||
            ammoPivot == null)
        {
            return;
        }

        Quaternion targetRotation = isFiring
            ? ammoRestRotation * Quaternion.Euler(firingTilt)
            : ammoRestRotation;

        ammoPivot.localRotation = Quaternion.Slerp(
            ammoPivot.localRotation,
            targetRotation,
            ammoTiltSpeed * Time.deltaTime
        );
    }

    public void Shoot()
    {
        if (!isActiveAndEnabled ||
            !isControlled ||
            isReloading)
        {
            return;
        }

        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            StopFiring();
            TryReload();
            return;
        }

        nextFireTime = Time.time + fireRate;

        FireShot();

        if (weaponType == WeaponType.MachineGun)
            isFiring = true;
    }

    private void FireShot()
    {
        currentAmmo--;

        PlayMuzzleFlashes();
        PlayFireAnimation();

        if (playerLook != null)
            playerLook.AddRecoil(cameraRecoil);

        if (playerCamera == null)
        {
            Debug.LogWarning(
                "Gun: Player Camera is not assigned.", this);
            return;
        }

        // Camera sets the bullet direction.
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        Vector3 hitPoint =
            ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            hitPoint = hit.point;

            EnemyHealth enemyHealth =
                hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage);

            SpawnBulletDecal(hit);
        }

        // Tracer starts at the muzzle if assigned.
        Vector3 tracerStart = muzzlePoint != null
            ? muzzlePoint.position
            : ray.origin;

        SpawnTracer(tracerStart, hitPoint);
    }

    private void PlayMuzzleFlashes()
    {
        if (muzzleFlashes == null)
            return;

        foreach (ParticleSystem flash in muzzleFlashes)
        {
            if (flash != null)
                flash.Play();
        }
    }

    private void PlayFireAnimation()
    {
        if (weaponAnimator != null &&
            !string.IsNullOrEmpty(fireAnimationTrigger))
        {
            weaponAnimator.SetTrigger(fireAnimationTrigger);
        }
    }

    private void SpawnBulletDecal(RaycastHit hit)
{
    if (bulletDecalPrefab == null)
        return;

    // Align decal with the actual impact surface.
    Quaternion rotation = Quaternion.LookRotation(
        hit.normal
    );

    // Instantiate slightly above the surface.
    Vector3 position =
        hit.point + hit.normal * decalOffset;

    GameObject decal = Instantiate(
        bulletDecalPrefab,
        position,
        rotation
    );

    // Attach decal to the object that was hit.
    decal.transform.SetParent(
        hit.collider.transform,
        true
    );

    if (decalLifetime > 0f)
        Destroy(decal, decalLifetime);
}

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (tracerPrefab == null)
            return;

        LineRenderer tracer = Instantiate(tracerPrefab);

        tracer.transform.SetParent(null);
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;

        tracer.startWidth = tracerWidth;
        tracer.endWidth = tracerWidth * 0.5f;

        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);
        tracer.enabled = true;

        Destroy(tracer.gameObject, tracerDuration);
    }

    public void TryReload()
    {
        if (!isActiveAndEnabled ||
            !isControlled ||
            isReloading ||
            currentAmmo >= magSize)
        {
            return;
        }

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        StopFiring();

        Quaternion startRotation = transform.localRotation;

        Quaternion targetRotation =
            originalWeaponRotation *
            Quaternion.Euler(reloadRotationOffset);

        float halfReloadTime = reloadTime * 0.5f;
        float elapsed = 0f;

        while (elapsed < halfReloadTime)
        {
            elapsed += Time.deltaTime;

            transform.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                elapsed / Mathf.Max(halfReloadTime, 0.001f)
            );

            yield return null;
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, reloadTime - halfReloadTime)
        );

        currentAmmo = magSize;
        elapsed = 0f;

        while (elapsed < halfReloadTime)
        {
            elapsed += Time.deltaTime;

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                originalWeaponRotation,
                elapsed / Mathf.Max(halfReloadTime, 0.001f)
            );

            yield return null;
        }

        transform.localRotation = originalWeaponRotation;
        isReloading = false;
    }

    public void SetShooting(bool shooting)
    {
        isFiring =
            shooting &&
            weaponType == WeaponType.MachineGun &&
            isControlled &&
            !isReloading &&
            currentAmmo > 0;
    }

    public void StopFiring()
    {
        isFiring = false;
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;

        if (!controlled)
            StopFiring();
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMagazineSize()
    {
        return magSize;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public bool IsAutomatic()
    {
        return weaponType == WeaponType.MachineGun;
    }
}