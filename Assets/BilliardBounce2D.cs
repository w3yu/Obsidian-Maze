using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BilliardBounce2D : MonoBehaviour
{
    public float elasticity = 1.0f;         // 1 = perfectly elastic
    public float minSpeedForBounce = 0.01f; // ignore micro-collisions
    public bool preserveSpin = true;        // keep angular velocity

    Rigidbody2D rb;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void OnCollisionEnter2D(Collision2D col)
    {
        Vector2 v = rb.linearVelocity;               // Unity 6
        if (v.sqrMagnitude < minSpeedForBounce * minSpeedForBounce) return;

        // Take the first contact normal (outward from the other collider)
        Vector2 n = col.GetContact(0).normal;
        Vector2 reflected = Vector2.Reflect(v, n) * elasticity;

        rb.linearVelocity = reflected;               // set new velocity
        // optional: damp tiny penetration correction impulses by clearing forces
        // rb.AddForce(Vector2.zero, ForceMode2D.Impulse);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        // Optional: keep reflecting while pressed against a moving wall etc.
        // (usually not needed; remove if it feels too "springy")
    }
}
