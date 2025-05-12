using UnityEngine;
using UnityEngine.InputSystem;

public class ShootProjectile : MonoBehaviour {
    public Transform spawn; // spawnpoint of projectile
    public InputActionProperty shootAction; // action that triggers shoot
    public AudioSource shootAudioSource; // audio source for shooting
    public AudioClip shootSound; // shooting sound effect
    public float cooldownTime = 0.5f; // cooldown between shots
    private float lastShotTime = 0f; // tracks time from last shot

    public ProjectileSpawner projectileSpawner;

    // Update is called once per frame
    void Update() {
        if (shootAction.action.WasPressedThisFrame() && Time.time >= lastShotTime + cooldownTime) {
            projectileSpawner.SpawnProjectile(spawn.position, spawn.rotation);
            if (shootAudioSource != null && shootSound != null) {
                shootAudioSource.PlayOneShot(shootSound);
            }
            lastShotTime = Time.time;
        }
    }
}