using UnityEngine;

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
