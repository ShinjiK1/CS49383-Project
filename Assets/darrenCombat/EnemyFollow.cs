using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public NavMeshAgent enemy;
    public Transform player;

    [SerializeField] private float timer = 5;
    private float bulletTime;
    public GameObject enemyBullet;

    public Transform spawnPoint;
    public float enemySpeed;

    
    // Update is called once per frame
    void Update()
    {
        enemy.SetDestination(player.position);
        ShootAtPlayer();
    }

    void ShootAtPlayer()
    {
        bulletTime -= Time.deltaTime;
        if (bulletTime > 0) return;

        bulletTime = timer;

        // Instantiate the bullet at the spawn point
        GameObject bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;

        // Null check: Ensure the bullet was successfully instantiated
        if (bulletObj != null)
        {
            // Get the Rigidbody component of the bullet
            Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();

            // Apply force to shoot the bullet forward
            bulletRig.AddForce(bulletRig.transform.forward * enemySpeed, ForceMode.Impulse);

            // Optionally destroy the bullet after 3 seconds
            Destroy(bulletObj, 3f);
        }
        else
        {
            Debug.LogWarning("Bullet instantiation failed or bulletPrefab is not assigned.");
        }
    }


}