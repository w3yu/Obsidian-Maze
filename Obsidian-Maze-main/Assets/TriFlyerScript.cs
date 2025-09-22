using UnityEngine;

public class TriFlyerScript : MonoBehaviour
{
    [Header("Refs")]
    public Transform wingL;          // left wing transform
    public Transform wingR;          // right wing transform

    [Header("Horizontal Motion")]
    public float xAmplitude = 1.2f;  // sine amplitude on X
    public float xFreq = 0.4f;       // Hz, horizontal sway speed

    [Header("Vertical Bob (optional)")]
    public float yAmplitude = 0.4f;  // small vertical bob
    public float yFreq = 1.2f;       // Hz

    [Header("Wing Flap")]
    public float flapAmpDeg = 35f;   // wing rotation amplitude (deg)
    public float flapFreq = 3.0f;    // Hz, flap speed
    public float wingBaseDeg = 10f;  // base spread angle

    [Header("Difficulty Coupling (optional)")]
    public bool coupleToCameraSpeed = true; // tie speeds to camera scroll
    public float kHoriz = 0.05f;            // multipliers when coupling
    public float kFlap = 0.15f;

    private Vector3 startPos;
    private float t;

    void Start()
    {
        startPos = transform.position;

        // Make sure wings exist
        if (!wingL || !wingR) Debug.LogWarning("[TriFlyer] Wings not assigned.");
    }

    void Update()
    {
        t += Time.deltaTime;

        // Optional difficulty coupling: scale frequencies by time
        float speed = 0f;

        float xf = xFreq * (1f + kHoriz * speed);
        float yf = yFreq;
        float ff = flapFreq * (1f + kFlap * speed);

        // Position path: sinusoidal X + tiny Y bob around startPos
        float x = startPos.x + Mathf.Sin(2f * Mathf.PI * xf * t) * xAmplitude;
        float y = startPos.y + Mathf.Sin(2f * Mathf.PI * yf * t) * yAmplitude;
        transform.position = new Vector3(x, y, startPos.z);

        // Wing flapping: opposite-phase rotations
        float angle = wingBaseDeg + Mathf.Sin(2f * Mathf.PI * ff * t) * flapAmpDeg;
        if (wingL) wingL.localRotation = Quaternion.Euler(0, 0, +angle);
        if (wingR) wingR.localRotation = Quaternion.Euler(0, 0, -angle);
    }

    // Simple hit logic (trigger). Tag your player as "Player".
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // TODO: apply damage / restart / knockback
            Debug.Log("TriFlyer hit player!");
        }
    }
}
