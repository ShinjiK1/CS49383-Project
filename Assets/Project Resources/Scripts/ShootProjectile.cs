using UnityEngine;
using UnityEngine.InputSystem;

public class ShootProjectile : MonoBehaviour {
    public GameObject projectileObj;
    public Transform spawn;
    public float force = 10f;
    public float gravityScale = 1f;
    public float lifetime = 5f;
    public InputActionProperty shootAction;

    // Update is called once per frame
    void Update() {
        if (shootAction.action.WasPressedThisFrame()) {
            Shoot();
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
        Destroy(projectile, lifetime);
        projectile.AddComponent<ProjectileCollision>();
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

public class ProjectileCollision : MonoBehaviour {
    void OnCollisionEnter(Collision collision) {
        Destroy(gameObject);
    }
}