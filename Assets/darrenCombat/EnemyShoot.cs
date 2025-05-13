using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject bulletPrefab; // Bullet prefab to be shot
    public Transform spawnPoint;    // The spawn point of the bullet (usually inside the enemy)
    public float bulletSpeed = 10f; // Speed of the bullet
    public float shootDelay = 2f;   // Delay between each shot
    private float nextShotTime = 0f; // Time when the next shot is allowed

    void Update()
    {
        // Check if enough time has passed to shoot
        if (Time.time >= nextShotTime)
        {
            ShootBullet();
            nextShotTime = Time.time + shootDelay; // Set the next shot time
        }
    }

    void ShootBullet()
    {
        // Check if the bulletPrefab is assigned in the Inspector
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned in the Inspector!");
            return;
        }

        // Instantiate the bullet at the spawn point
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        // Check if the bullet has a Rigidbody component
        Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();
        if (bulletRb == null)
        {
            Debug.LogError("Rigidbody component is missing from the bullet prefab!");
            return;
        }

        // Apply a forward force to the bullet (shooting it straight ahead)
        bulletRb.AddForce(spawnPoint.forward * bulletSpeed, ForceMode.Impulse);

        // Optionally destroy the bullet after a set time (e.g., 5 seconds)
        Destroy(bulletObj, 5f); // Destroys the bullet after 5 seconds
    }
}