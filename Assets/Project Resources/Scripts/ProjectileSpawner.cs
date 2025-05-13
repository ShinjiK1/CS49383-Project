using UnityEngine;
using System.Collections.Generic;

public class ProjectileSpawner : MonoBehaviour {
    public List<SpellData> spells;

    public void SpawnProjectile(Vector3 position, Quaternion rotation, int spellID) {
        if(spellID < 0 || spellID >= spells.Count) {
            Debug.LogWarning($"Invalid spellID: {spellID}");
            return;
        }

        SpellData spell = spells[spellID];
        GameObject projectile = Instantiate(spell.projectilePrefab, position, rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = transform.forward * spell.force;
            rb.useGravity = false;
            CustomGravity gravity = projectile.AddComponent<CustomGravity>();
            gravity.gravityScale = spell.gravityScale;
        }

        Destroy(projectile, spell.lifetime);
        ProjectileCollision collision = projectile.AddComponent<ProjectileCollision>();
        collision.collisionSound = spell.collisionSound;
    }
}
