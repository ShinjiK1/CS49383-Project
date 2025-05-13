using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    public float gravityScale = 1f;
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        if(rb != null) {
            rb.linearVelocity += Physics.gravity * gravityScale * Time.fixedDeltaTime;
        }
    }
}
