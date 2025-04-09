using UnityEngine;
using UnityEngine.InputSystem;

public class ShootProjectile : MonoBehaviour {
    public GameObject projectileObj; // projectile to be spawned
    public Transform spawn; // spawnpoint of projectile
    public float force = 10f; // force of object
    public float gravityScale = 1f; // used to control how much object is affected by gravity
    public float lifetime = 5f; // default time before object despawns
    public float cooldownTime = 0.5f; // cooldown between shots
    private float lastShotTime = 0f; // tracks time from last shot
    public InputActionProperty shootAction; // action that triggers shoot
    public AudioSource shootAudioSource; // audio source for shooting
    public AudioClip shootSound; // shooting sound effect
    public AudioClip collisionSound; // collision sound effect

    // Update is called once per frame
    void Update() {
        if (shootAction.action.WasPressedThisFrame() && Time.time >= lastShotTime + cooldownTime) {
            Shoot();
            lastShotTime = Time.time;
        }
    }
    void Shoot() {
        GameObject projectile = Instantiate(projectileObj, spawn.position, spawn.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = spawn.forward * force;
            rb.useGravity = false;
            projectile.AddComponent<CustomGravity>().gravityScale = gravityScale;
        }

        if (shootAudioSource != null && shootSound != null) {
            shootAudioSource.PlayOneShot(shootSound);
        }

        Destroy(projectile, lifetime);
        ProjectileCollision projectileCollision = projectile.AddComponent<ProjectileCollision>();
        projectileCollision.collisionSound = collisionSound;
    }
}

// Class for custom gravity
public class CustomGravity : MonoBehaviour {
    public float gravityScale = 1f;
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        if (rb != null) {
            rb.linearVelocity += Physics.gravity * gravityScale * Time.fixedDeltaTime;
        }
    }
}

public class ProjectileCollision : MonoBehaviour
{
    public AudioClip collisionSound;

    void OnCollisionEnter(Collision collision) {
        if (collisionSound != null) {
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
        }
        Destroy(gameObject);
    }
}