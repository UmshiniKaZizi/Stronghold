using UnityEngine;

public class ArtilleryBombLauncher : MonoBehaviour
{
    [Header("Bomb Prefab")]
    [SerializeField] private GameObject bombPrefab;

    [Header("Launch Point")]
    [SerializeField] private Transform bulletSpawner;

    [Header("Inventory")]
    [SerializeField] private ArtilleryBombInventory bombInventory;

    [Header("Launch Settings")]
    [SerializeField] private float launchSpeed = 25f;
    [SerializeField] private float upwardForce = 3f;

    [Header("Firing")]
    [SerializeField] private float fireCooldown = 1f;

    private float nextFireTime;

    public bool TryFire()
    {
        // Check required references.
        if (bombPrefab == null)
        {
            Debug.LogError("ArtilleryLauncher: Bomb prefab is missing!", this);
            return false;
        }

        if (bulletSpawner == null)
        {
            Debug.LogError("ArtilleryLauncher: BulletSpawner is missing!", this);
            return false;
        }

        if (bombInventory == null)
        {
            Debug.LogError("ArtilleryLauncher: Inventory is missing!", this);
            return false;
        }

        // Check inventory.
        if (!bombInventory.HasBomb)
        {
            Debug.Log("ArtilleryLauncher: No bombs available.");
            return false;
        }

        // Check cooldown.
        if (Time.time < nextFireTime)
            return false;

        // Calculate launch direction.
        Vector3 launchDirection =
            (bulletSpawner.forward +
             bulletSpawner.up * (upwardForce / launchSpeed)).normalized;

        // Spawn the bomb at the muzzle.
        GameObject bomb = Instantiate(
            bombPrefab,
            bulletSpawner.position,
            Quaternion.LookRotation(launchDirection)
        );

        ArtilleryBombProjectile projectile =
            bomb.GetComponent<ArtilleryBombProjectile>();

        if (projectile == null)
        {
            Debug.LogError(
                "ArtilleryLauncher: Prefab has no ArtilleryBombProjectile component!",
                bomb
            );

            Destroy(bomb);
            return false;
        }

        // Launch the projectile.
        projectile.Launch(
            launchDirection * launchSpeed,
            transform
        );

        // Consume ammunition only after successful spawning.
        if (!bombInventory.ConsumeBomb())
        {
            Destroy(bomb);
            return false;
        }

        nextFireTime = Time.time + fireCooldown;

        Debug.Log(
            "ARTILLERY FIRED | Remaining bombs: " +
            bombInventory.CurrentBombs
        );

        return true;
    }

    // Temporary testing method.
    [ContextMenu("TEST FIRE BOMB")]
    private void TestFireBomb()
    {
        TryFire();
    }
}
