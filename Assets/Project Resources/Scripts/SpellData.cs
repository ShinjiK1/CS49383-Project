using UnityEngine;

[System.Serializable]
public class SpellData
{
    public GameObject projectilePrefab; // projectiles to be spawned
    public float force = 10f; // force of object
    public float gravityScale = 1f; // used to control how much object is affected by gravity
    public float lifetime = 5f; // default time before object despawns
    public AudioClip collisionSound; // collision sound effect
}
