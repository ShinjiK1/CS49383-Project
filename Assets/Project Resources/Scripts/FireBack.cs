using UnityEngine;
using UnityEngine.Audio;

public class FireOnCollision : MonoBehaviour
{
    public GameObject sphereProjectilePrefab;
    public float projectileSpeed = 10f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (sphereProjectilePrefab != null)
        {
            if (audioSource != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.Play();
            }
            Vector3 spawnPosition = transform.position + transform.forward * 1f;
            GameObject projectile = Instantiate(sphereProjectilePrefab, spawnPosition, Quaternion.identity);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * projectileSpeed;
            }

            Destroy(projectile, 5f); // <- Destroys it after 5 seconds

        }
        else
        {
            Debug.LogWarning("Sphere projectile prefab is not assigned.");
        }
    }
}
