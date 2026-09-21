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

        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }

        // Hide tracer when game starts
        if (bulletTracer != null)
        {
            bulletTracer.enabled = false;
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

        FireRaycast();

        // Fire animation
        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }

        // Weapon recoil currently disabled
        // ApplyRecoil();

        // Camera recoil
        if (playerLook != null)
        {
            playerLook.AddRecoil(cameraRecoil);
        }

        // Play all muzzle flashes
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

        /*Debug.Log(
            "SHOT FIRED | Ammo: " +
            currentAmmo
        );*/
    }

    private void FireRaycast()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning(
                "Player Camera is not assigned!"
            );

            return;
        }

        // Create the actual aiming ray from the
        // center of the player's camera.
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        // Default endpoint if nothing is hit.
        Vector3 traceEnd =
            ray.origin +
            ray.direction * range;

        // Check what the player is aiming at.
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            range))
        {
            traceEnd = hit.point;

            /*Debug.Log(
                "HIT: " +
                hit.collider.gameObject.name +
                " | Distance: " +
                hit.distance
            );*/

            // Enemy damage
            EnemyHealth enemyHealth =
                hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            // Bullet decal
            SpawnBulletDecal(hit);
        }
        else
        {
           // Debug.Log("SHOT MISSED");
        }

        // ---------------------------------
        // BULLET TRACE
        // ---------------------------------

        Vector3 traceStart;

        if (bulletTraceOrigin != null)
        {
            traceStart = bulletTraceOrigin.position;
        }
        else
        {
            traceStart = transform.position;
        }

        if (bulletTracer != null)
        {
            StartCoroutine(
                ShowBulletTracer(
                    traceStart,
                    traceEnd
                )
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

        bulletTracer.SetPosition(
            0,
            start
        );

        bulletTracer.SetPosition(
            1,
            end
        );

        yield return new WaitForSeconds(
            tracerDuration
        );

        bulletTracer.enabled = false;
    }

    private void SpawnBulletDecal(RaycastHit hit)
    {
        if (bulletDecalPrefab == null)
        {
            Debug.LogWarning(
                "Bullet Decal Prefab is not assigned!"
            );

            return;
        }

        Vector3 position =
            hit.point +
            hit.normal * decalOffset;

        Quaternion rotation =
            Quaternion.LookRotation(hit.normal);

        GameObject decal = Instantiate(
            bulletDecalPrefab,
            position,
            rotation
        );

        decal.transform.SetParent(
            hit.collider.transform
        );

        Destroy(
            decal,
            decalLifetime
        );
    }

    private void ApplyRecoil()
    {
        recoilTargetRotation *=
            Quaternion.Euler(
                -recoilRotation,
                0f,
                0f
            );

        recoilTargetPosition +=
            new Vector3(
                0f,
                0f,
                -recoilKickback
            );
    }

    private void HandleRecoil()
    {
        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                recoilTargetRotation,
                recoilSnappiness *
                Time.deltaTime
            );

        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                recoilTargetPosition,
                recoilSnappiness *
                Time.deltaTime
            );

        recoilTargetRotation =
            Quaternion.Slerp(
                recoilTargetRotation,
                initialRotation,
                recoilReturnSpeed *
                Time.deltaTime
            );

        recoilTargetPosition =
            Vector3.Lerp(
                recoilTargetPosition,
                initialPosition,
                recoilReturnSpeed *
                Time.deltaTime
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
            Quaternion.Euler(
                reloadRotationOffset
            );

        float elapsed = 0f;

        // Rotate weapon down
        while (elapsed < reloadTime / 2f)
        {
            elapsed += Time.deltaTime;

            float t =
                elapsed /
                (reloadTime / 2f);

            transform.localRotation =
                Quaternion.Slerp(
                    initialRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        // Refill magazine
        currentAmmo = magSize;

        elapsed = 0f;

        // Rotate weapon back
        while (elapsed < reloadTime / 2f)
        {
            elapsed += Time.deltaTime;

            float t =
                elapsed /
                (reloadTime / 2f);

            transform.localRotation =
                Quaternion.Slerp(
                    targetRotation,
                    initialRotation,
                    t
                );

            yield return null;
        }

        transform.localRotation =
            initialRotation;

        transform.localPosition =
            initialPosition;

        recoilTargetRotation =
            initialRotation;

        recoilTargetPosition =
            initialPosition;

        isReloading = false;

        /*Debug.Log(
            "RELOAD COMPLETE | Ammo: " +
            currentAmmo
        );*/
    }
}