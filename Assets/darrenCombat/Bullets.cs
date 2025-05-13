using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;

    private void Start()
    {
        // Destroy the bullet after 10 seconds to avoid memory bloat
        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object collided with has the tag "Player"
        if (other.CompareTag("Player"))
        {
            Debug.LogWarning("bullet hit player");
            // Make sure the PlayerStats component exists before accessing it
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Apply damage to the player
                playerStats.TakeDamage(damage);
            }
            else
            {
                // Log warning if PlayerStats is missing
                Debug.LogWarning("PlayerStats component missing on the player!");
            }
            // Destroy the bullet after it hits the player
            Destroy(gameObject);
        }
    }
}