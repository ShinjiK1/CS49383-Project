using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    public AudioClip collisionSound;
    public GameObject onHitPrefab;
    public float lifetime;
    public float soundDuration;

    void OnCollisionEnter(Collision collision) {
        if (collisionSound != null) {
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = transform.position;

            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
            audioSource.clip = collisionSound;
            audioSource.Play();

            Destroy(tempAudio, soundDuration);
        }

        if (onHitPrefab != null) {
            GameObject spawnedEffect = Instantiate(onHitPrefab, transform.position, Quaternion.identity);
            Destroy(spawnedEffect, lifetime);
        }
        Destroy(gameObject);
    }
}
