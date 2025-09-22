using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDrag2D : MonoBehaviour
{
    public enum MouseButton { Left, Right, Middle }

    [Header("Basics")]
    public Camera cam;                     // auto-filled to Camera.main if empty
    public bool enableMouseDrag = true;
    public MouseButton dragMouseButton = MouseButton.Left;
    public bool enableTouchDrag = true;    // one-finger pan on touch
    public float dragSpeed = 1f;           // global multiplier

    [Header("Bounds")]
    public bool clampToBounds = true;
    public Collider2D boundsCollider;      // Box/Polygon/Composite that encloses the world

    [Header("Zoom (optional)")]
    public bool allowZoom = false;
    public float zoomSpeed = 1f;           // wheel sensitivity
    public float minOrthoSize = 3f;
    public float maxOrthoSize = 20f;

    void Awake()
    {
        if (!cam) cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
        if (cam) cam.orthographic = true;  // ensure 2D
    }

    void Update()
    {
        if (!cam) return;

        // --- 1) accumulate pointer delta in screen pixels ---
        Vector2 deltaPx = Vector2.zero;

        // Mouse
        if (enableMouseDrag && Mouse.current != null && IsMousePressed(dragMouseButton))
            deltaPx += Mouse.current.delta.ReadValue();

        // Touch (primary finger)
        if (enableTouchDrag && Touchscreen.current != null)
        {
            var t = Touchscreen.current.primaryTouch;
            if (t.press.isPressed) deltaPx += t.delta.ReadValue();
        }

        // --- 2) convert pixel delta -> world delta (orthographic) ---
        if (deltaPx.sqrMagnitude > 0f)
        {
            // world units per pixel
            float unitsPerPixelY = (cam.orthographicSize * 2f) / Screen.height;
            float unitsPerPixelX = unitsPerPixelY * cam.aspect;

            // drag camera opposite to pointer movement (hand tool behavior)
            Vector3 worldDelta = new Vector3(-deltaPx.x * unitsPerPixelX,
                                             -deltaPx.y * unitsPerPixelY,
                                             0f) * dragSpeed;

            cam.transform.position += worldDelta;
            if (clampToBounds && boundsCollider) ClampToBounds();
        }

        // --- 3) optional zoom by mouse wheel ---
        if (allowZoom && Mouse.current != null)
        {
            float scrollY = Mouse.current.scroll.ReadValue().y; // pixels per frame
            if (Mathf.Abs(scrollY) > 0.01f)
            {
                // exponential zoom for smoothness
                float factor = Mathf.Exp(scrollY * 0.001f * zoomSpeed);
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize / factor, minOrthoSize, maxOrthoSize);
                if (clampToBounds && boundsCollider) ClampToBounds();
            }
        }
    }

    bool IsMousePressed(MouseButton b)
    {
        if (Mouse.current == null) return false;
        switch (b)
        {
            case MouseButton.Left: return Mouse.current.leftButton.isPressed;
            case MouseButton.Middle: return Mouse.current.middleButton.isPressed;
            default: return Mouse.current.rightButton.isPressed;
        }
    }

    void ClampToBounds()
    {
        if (!boundsCollider) return;

        Bounds b = boundsCollider.bounds;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 p = cam.transform.position;
        p.x = Mathf.Clamp(p.x, b.min.x + halfW, b.max.x - halfW);
        p.y = Mathf.Clamp(p.y, b.min.y + halfH, b.max.y - halfH);
        cam.transform.position = p;
    }
}
