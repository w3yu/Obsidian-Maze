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

    [Header("Ball Following")]
    public bool followBall = true;
    public string ballTag = "Ball";
    public float ballFollowThreshold = 0.3f;  // 30% up the screen
    public float ballFallThreshold = 0.1f;    // 10% from the bottom of the screen
    
    [Header("Bounds")]
    public bool clampToBounds = true;
    public Collider2D boundsCollider;      // Box/Polygon/Composite that encloses the world

    [Header("Zoom (optional)")]
    public bool allowZoom = false;
    public float zoomSpeed = 1f;           // wheel sensitivity
    public float minOrthoSize = 3f;
    public float maxOrthoSize = 20f;

    private Transform ballTransform;
    private Vector3 dragOffset = Vector3.zero;  // User's manual camera offset
    private bool isDragging = false;

    void Awake()
    {
        if (!cam) cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
        if (cam) cam.orthographic = true;  // ensure 2D
        
        // Set camera size to show full game width
        SetCameraToFitWidth();
        
        // Find the ball
        GameObject ballObject = GameObject.FindGameObjectWithTag(ballTag);
        if (ballObject) ballTransform = ballObject.transform;
    }

    void Start()
{
        // Ensure camera is properly sized at start
        SetCameraToFitWidth();
    }


    void SetCameraToFitWidth()
    {
        // Auto-detect game width based on level bounds
        float detectedWidth = AutoDetectGameWidth();
        
        // Calculate the orthographic size needed to show the full detected width
        float aspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = detectedWidth / (2f * aspect);
        
        Debug.Log($"Auto-detected game width: {detectedWidth}, Camera orthographic size set to: {cam.orthographicSize}");
    }

    float AutoDetectGameWidth()
    {
        // Method 1: Use bounds collider if available
        if (boundsCollider != null)
        {
            return boundsCollider.bounds.size.x;
        }

        // Method 2: Find all renderers and calculate bounds
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        if (allRenderers.Length > 0)
        {
            Bounds combinedBounds = allRenderers[0].bounds;
            foreach (Renderer renderer in allRenderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
            return combinedBounds.size.x;
        }

        // Method 3: Find all colliders and calculate bounds
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        if (allColliders.Length > 0)
        {
            Bounds combinedBounds = allColliders[0].bounds;
            foreach (Collider2D collider in allColliders)
            {
                combinedBounds.Encapsulate(collider.bounds);
            }
            return combinedBounds.size.x;
        }

        // Fallback: use a reasonable default
        return 20f;
    }


    void Update()
    {
        if (!cam) return;

        // --- 1) accumulate pointer delta in screen pixels ---
        Vector2 deltaPx = Vector2.zero;
        bool currentlyDragging = false;

        // Mouse
        if (enableMouseDrag && Mouse.current != null && IsMousePressed(dragMouseButton))
        {
            deltaPx += Mouse.current.delta.ReadValue();
            currentlyDragging = true;
        }

        // Touch (primary finger)
        if (enableTouchDrag && Touchscreen.current != null)
        {
            var t = Touchscreen.current.primaryTouch;
            if (t.press.isPressed)
            {
                deltaPx += t.delta.ReadValue();
                currentlyDragging = true;
            }
        }

        // --- 2) Handle manual dragging ---
        Vector3 worldDelta = Vector3.zero;
        if (deltaPx.sqrMagnitude > 0f)
        {
            isDragging = true;
            
            // world units per pixel
            float unitsPerPixelY = (cam.orthographicSize * 2f) / Screen.height;
            float unitsPerPixelX = unitsPerPixelY * cam.aspect;

            // drag camera opposite to pointer movement (hand tool behavior)
            worldDelta = new Vector3(-deltaPx.x * unitsPerPixelX,
                                     -deltaPx.y * unitsPerPixelY,
                                     0f) * dragSpeed;

            // Update the drag offset instead of moving camera directly
            dragOffset += worldDelta;
        }
        
        // Stop dragging when not pressing
        if (!currentlyDragging)
        {
            isDragging = false;
        }

        // --- 3) Ball following logic ---
        if (followBall && ballTransform)
        {
            Vector3 targetPosition = CalculateTargetCameraPosition();
            cam.transform.position = targetPosition + dragOffset;
        }
        else if (isDragging)
        {
            // If not following ball, use traditional drag behavior
            cam.transform.position += worldDelta;
        }

        // Apply bounds clamping
        if (clampToBounds && boundsCollider) ClampToBounds();

        // --- 4) optional zoom by mouse wheel ---
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

    Vector3 CalculateTargetCameraPosition()
    {
        Vector3 ballPos = ballTransform.position;
        Vector3 currentCameraPos = cam.transform.position;
        
        // Convert ball position to screen coordinates
        Vector3 ballScreenPos = cam.WorldToViewportPoint(ballPos);
        
        // Check if ball is above the upper threshold (30% up the screen)
        if (ballScreenPos.y >= ballFollowThreshold)
        {
            // Calculate where camera should be to keep ball at the upper threshold position
            float targetY = ballPos.y - (cam.orthographicSize * 2f * ballFollowThreshold - cam.orthographicSize);
            return new Vector3(currentCameraPos.x, targetY, currentCameraPos.z);
        }
        // Check if ball is below the lower threshold (10% from bottom)
        else if (ballScreenPos.y < ballFallThreshold)
        {
            // Calculate where camera should be to keep ball at the lower threshold position
            float targetY = ballPos.y - (cam.orthographicSize * 2f * ballFallThreshold - cam.orthographicSize);
            return new Vector3(currentCameraPos.x, targetY, currentCameraPos.z);
        }
        
        // If ball is within both thresholds, don't move camera vertically
        return currentCameraPos;
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
