
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

    [Header("Camera Recoil")]
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private float cameraRecoil = 1.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Reload Animation")]
    [SerializeField] private Vector3 reloadRotationOffset =
        new Vector3(66f, 50f, 50f);

    [Header("Bullet Decal")]
    [SerializeField] private GameObject bulletDecalPrefab;
    [SerializeField] private float decalOffset = 0.01f;
    [SerializeField] private float decalLifetime = 5f;

    [Header("Bullet Trace")]
    [SerializeField] private LineRenderer bulletTracer;
    [SerializeField] private Transform bulletTraceOrigin;
    [SerializeField] private float tracerDuration = 0.05f;

    private int currentAmmo;
    private bool isReloading;
    private float nextTimeToFire;

    private Quaternion initialRotation;
    private Coroutine reloadCoroutine;

    private void Awake()
    {
        initialRotation = transform.localRotation;

        if (playerCamera == null)
        {
            playerCamera = GetComponentInParent<Camera>();

            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        if (playerLook == null)
            playerLook = GetComponentInParent<PlayerLook>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    private void Start()
    {
        currentAmmo = magSize;

        if (bulletTracer != null)
            bulletTracer.enabled = false;
    }

    public void Shoot()
    {
        if (isReloading)
            return;

        if (Time.time < nextTimeToFire)
            return;

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        nextTimeToFire = Time.time + fireRate;
        currentAmmo--;

        FireRaycast();

        if (animator != null)
            animator.SetTrigger("Fire");

        if (playerLook != null)
            playerLook.AddRecoil(cameraRecoil);

        if (muzzleFlashes != null)
        {
            foreach (ParticleSystem flash in muzzleFlashes)
            {
                if (flash != null)
                    flash.Play();
            }
        }
    }

    private void FireRaycast()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning(
                "Gun: Player Camera is not assigned."
            );
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 traceEnd = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            traceEnd = hit.point;

            EnemyHealth enemyHealth =
                hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage);

            SpawnBulletDecal(hit);
        }

        Vector3 traceStart = bulletTraceOrigin != null
            ? bulletTraceOrigin.position
            : transform.position;

        if (bulletTracer != null)
        {
            StartCoroutine(
                ShowBulletTracer(traceStart, traceEnd)
            );
        }
    }

    private IEnumerator ShowBulletTracer(
        Vector3 start,
        Vector3 end)
    {
        if (bulletTracer == null)
            yield break;

        bulletTracer.enabled = true;
        bulletTracer.SetPosition(0, start);
        bulletTracer.SetPosition(1, end);

        yield return new WaitForSeconds(tracerDuration);

        if (bulletTracer != null)
            bulletTracer.enabled = false;
    }

    private void SpawnBulletDecal(RaycastHit hit)
    {
        if (bulletDecalPrefab == null)
            return;

        Vector3 position =
            hit.point + hit.normal * decalOffset;

        Quaternion rotation =
            Quaternion.LookRotation(hit.normal);

        GameObject decal = Instantiate(
            bulletDecalPrefab,
            position,
            rotation
        );

        decal.transform.SetParent(hit.collider.transform, true);

        Destroy(decal, decalLifetime);
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo >= magSize)
            return;

        reloadCoroutine = StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        Quaternion startRotation = transform.localRotation;

        Quaternion targetRotation =
            initialRotation * Quaternion.Euler(reloadRotationOffset);

        float halfReloadTime = Mathf.Max(reloadTime * 0.5f, 0.01f);
        float elapsed = 0f;

        // Rotate the weapon down during the first half.
        while (elapsed < halfReloadTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / halfReloadTime);

            transform.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        currentAmmo = magSize;
        elapsed = 0f;

        // Return the weapon during the second half.
        while (elapsed < halfReloadTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / halfReloadTime);

            transform.localRotation = Quaternion.Slerp(
                targetRotation,
                initialRotation,
                t
            );

            yield return null;
        }

        transform.localRotation = initialRotation;

        isReloading = false;
        reloadCoroutine = null;
    }
}
