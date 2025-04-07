using UnityEngine;
using UnityEngine.InputSystem;

public class ShootProjectile : MonoBehaviour
{
    public GameObject projectileObj;
    public Transform spawn;
    public float force = 10f;
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
        }
    }
}
