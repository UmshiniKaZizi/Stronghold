using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float reloadTime = 1f;
    public float fireRate = 0.15f;
    public int magSize = 20;

    [Header("Bullet")]
    public GameObject bullet;
    public Transform bulletSpawnPoint;

    [Header("Reload Animation")]
    public Vector3 reloadRotationOffset = new Vector3(66f, 50f, 50f);

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    private Quaternion initialRotation;
    private Vector3 initialPosition;

    void Start()
    {
        currentAmmo = magSize;

        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;
    }

    public void Shoot()
    {
        // Can't shoot while reloading
        if (isReloading)
            return;

        // Fire-rate limiter
        if (Time.time < nextTimeToFire)
            return;

        // Automatically reload when magazine is empty
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // Set next allowed firing time
        nextTimeToFire = Time.time + fireRate;

        // Consume ammo
        currentAmmo--;

        // Spawn bullet
        Instantiate(
            bullet,
            bulletSpawnPoint.position,
            bulletSpawnPoint.rotation
        );

        Debug.Log("Shot fired. Ammo: " + currentAmmo);
    }

    IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("Reloading...");

        // Calculate reload rotation
        Quaternion targetRotation = initialRotation * Quaternion.Euler(reloadRotationOffset);

        float elapsed = 0f;

        // Animate gun into reload position
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

        // Animate gun back to original position
        elapsed = 0f;

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

        // Make absolutely sure we are back at the original rotation
        transform.localRotation = initialRotation;

        isReloading = false;

        Debug.Log("Reload complete. Ammo: " + currentAmmo);
    }

    public void TryReload()
    {
        if (isReloading) return;
        if(currentAmmo == magSize) return;

        StartCoroutine(Reload());
    }
}