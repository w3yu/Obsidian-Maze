using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLauncher : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D ball;
    public CircleCollider2D ballCollider;
    public Transform arrow;

    public enum LengthAxis { X, Y }

    [Header("Aim & Power")]
    public float angleDeg = 0f;
    public float angleSpeed = 120f;

    // keep these if you ever want to limit rotation; ignored when freeRotate=true
    public float minAngle = -170f;
    public float maxAngle = 170f;

    public float power = 5f;
    public float minPower = 1f;
    public float maxPower = 20f;
    public float powerChangePerSec = 10f;

    [Header("Arrow Visual")]
    public LengthAxis arrowLengthAxis = LengthAxis.Y; // your sprite points UP => Y
    public float modelForwardAngleOffsetDeg = 90f;    // UP sprite => +90
    public float arrowLengthPerPower = 0.2f;
    public float minArrowLength = 0.5f;
    public float gapFromBall = 0.06f;
    public bool arrowPivotIsCenter = true;
    public bool hideArrowAfterFire = true;

    [Header("Controls / Rearm")]
    public bool invertHorizontal = true;        // reverse L/R if needed
    public bool freeRotate = true;              // <<< NEW: allow unlimited rotation
    public bool autoRearmWhenStopped = true;
    public float rearmSpeedThreshold = 0.05f;
    public float rearmSettleTime = 0.2f;
    public Key rearmKey = Key.R;

    // Input (code-bound)
    private InputAction aimPowerAction;
    private InputAction fireAction;

    private bool fired = false;
    private Vector3 baseScale;
    private float stoppedTimer = 0f;

    void Awake()
    {
        if (!ballCollider && ball) ballCollider = ball.GetComponent<CircleCollider2D>();
        if (arrow) baseScale = arrow.localScale;
        if (ball) { ball.gravityScale = 0f; ball.freezeRotation = true; }

        aimPowerAction = new InputAction("AimPower", InputActionType.Value, expectedControlType: "Vector2");
        aimPowerAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        aimPowerAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        aimPowerAction.AddBinding("<Gamepad>/leftStick");

        fireAction = new InputAction("Fire", InputActionType.Button, "<Keyboard>/space");
        fireAction.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable() { aimPowerAction.Enable(); fireAction.Enable(); }
    void OnDisable() { fireAction.Disable(); aimPowerAction.Disable(); }

    void Update()
    {
        if (!enabled || !gameObject.activeInHierarchy) return;

        Vector2 ap = aimPowerAction.ReadValue<Vector2>();
        float dt = Time.unscaledDeltaTime;

        if (fired)
        {
            if (Keyboard.current != null && Keyboard.current[rearmKey].wasPressedThisFrame)
            {
                ArmAgain(ball.position, angleDeg, power);
                return;
            }

            if (autoRearmWhenStopped)
            {
                float speed2 = ball.linearVelocity.sqrMagnitude;
                if (speed2 <= rearmSpeedThreshold * rearmSpeedThreshold)
                {
                    stoppedTimer += dt;
                    if (stoppedTimer >= rearmSettleTime)
                    {
                        ArmAgain(ball.position, angleDeg, power);
                        return;
                    }
                }
                else stoppedTimer = 0f;
            }
            return;
        }

        // --- Aim & power ---
        float hx = ap.x * (invertHorizontal ? -1f : 1f);

        if (freeRotate)
        {
            // wrap to (-180, 180] so it can spin forever in either direction
            angleDeg = NormalizeAngleDeg(angleDeg + hx * angleSpeed * dt);
        }
        else
        {
            angleDeg = Mathf.Clamp(angleDeg + hx * angleSpeed * dt, minAngle, maxAngle);
        }

        power = Mathf.Clamp(power + ap.y * powerChangePerSec * dt, minPower, maxPower);

        UpdateArrow();

        if (fireAction.WasPressedThisFrame())
            Fire();
    }

    void UpdateArrow()
    {
        if (!arrow || !ball) return;

        float visLen = Mathf.Max(minArrowLength, power * arrowLengthPerPower);
        arrow.rotation = Quaternion.Euler(0f, 0f, angleDeg + modelForwardAngleOffsetDeg);

        Vector3 scale = baseScale;
        if (arrowLengthAxis == LengthAxis.X) scale.x = baseScale.x * visLen;
        else scale.y = baseScale.y * visLen;
        arrow.localScale = scale;

        float radius = GetBallWorldRadius();
        float halfLenShift = arrowPivotIsCenter ? (visLen * 0.5f) : 0f;
        Vector3 dir = (arrowLengthAxis == LengthAxis.X) ? arrow.right : arrow.up;

        Vector3 basePos = new Vector3(ball.position.x, ball.position.y, arrow.position.z);
        arrow.position = basePos + dir * (radius + gapFromBall + halfLenShift);
    }

    void Fire()
    {
        if (!ball || fired) return;

        Vector2 dir = AngleToDir(angleDeg);
        ball.linearVelocity = Vector2.zero;
        ball.angularVelocity = 0f;
        ball.AddForce(dir * power, ForceMode2D.Impulse);

        fired = true;
        stoppedTimer = 0f;
        if (hideArrowAfterFire && arrow) arrow.gameObject.SetActive(false);
    }

    // --- helpers ---
    static float NormalizeAngleDeg(float a) => Mathf.Repeat(a + 180f, 360f) - 180f; // (-180,180]
    static Vector2 AngleToDir(float deg) { float r = deg * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(r), Mathf.Sin(r)); }

    float GetBallWorldRadius()
    {
        if (ballCollider)
        {
            float s = Mathf.Max(ball.transform.lossyScale.x, ball.transform.lossyScale.y);
            return ballCollider.radius * s;
        }
        var sr = ball ? ball.GetComponent<SpriteRenderer>() : null;
        if (sr && sr.sprite)
        {
            float s = Mathf.Max(ball.transform.lossyScale.x, ball.transform.lossyScale.y);
            return Mathf.Max(sr.sprite.bounds.extents.x, sr.sprite.bounds.extents.y) * s;
        }
        return 0.5f;
    }

    public void ArmAgain(Vector3 newBallPos, float newAngle = 0f, float newPower = 5f)
    {
        fired = false;
        stoppedTimer = 0f;
        ball.position = newBallPos;
        angleDeg = newAngle;
        power = Mathf.Clamp(newPower, minPower, maxPower);
        if (arrow) arrow.gameObject.SetActive(true);
        UpdateArrow();
    }
}
