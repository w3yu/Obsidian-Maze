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
    public float ballFollowThreshold = 0.15f;  // 15% from top (more aggressive following)
    public float ballFallThreshold = 0.05f;    // 5% from bottom (keep ball more centered)
    public float ballHorizontalFollowThreshold = 0.1f;  // Follow when ball is 10% from screen edges
    public float followSmoothness = 8f;  // Faster camera response to prevent ball escaping
    public float edgeBuffer = 0.02f;  // 2% safety buffer to ensure ball never leaves screen
    
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
        
        // Set fixed camera width (20% narrower than tutorial level)
        SetFixedCameraWidth();
        
        // Find the ball
        GameObject ballObject = GameObject.FindGameObjectWithTag(ballTag);
        if (ballObject) ballTransform = ballObject.transform;
    }

    void Start()
    {
        // Ensure camera is properly sized at start
        SetFixedCameraWidth();
    }

    void SetFixedCameraWidth()
    {
        // Tutorial level width is approximately 20.5 units based on WorldBoundary scale
        // Making it 20% narrower means showing 80% of that width
        float tutorialLevelWidth = 20.5f;
        float fixedCameraWidth = tutorialLevelWidth * 0.8f; // 16.4 units wide
        
        // Calculate the orthographic size needed to show the fixed width
        float aspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = fixedCameraWidth / (2f * aspect);
        
        Debug.Log($"Fixed camera width: {fixedCameraWidth}, Camera orthographic size set to: {cam.orthographicSize}");
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
            Vector3 desiredPosition = targetPosition + dragOffset;
            
            // Smooth camera movement
            cam.transform.position = Vector3.Lerp(cam.transform.position, desiredPosition, Time.deltaTime * followSmoothness);
            
            // Apply modified bounds clamping that allows camera to reach edges for ball visibility
            if (clampToBounds && boundsCollider) ClampToBoundsForBall();
        }
        else if (isDragging)
        {
            // If not following ball, use traditional drag behavior
            cam.transform.position += worldDelta;
            
            // Apply normal bounds clamping when not following ball
            if (clampToBounds && boundsCollider) ClampToBounds();
        }

        // --- 4) optional zoom by mouse wheel ---
        if (allowZoom && Mouse.current != null)
        {
            float scrollY = Mouse.current.scroll.ReadValue().y; // pixels per frame
            if (Mathf.Abs(scrollY) > 0.01f)
            {
                // exponential zoom for smoothness
                float factor = Mathf.Exp(scrollY * 0.001f * zoomSpeed);
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize / factor, minOrthoSize, maxOrthoSize);
                if (clampToBounds && boundsCollider)
                {
                    if (followBall && ballTransform)
                        ClampToBoundsForBall();
                    else
                        ClampToBounds();
                }
            }
        }
    }

    Vector3 CalculateTargetCameraPosition()
    {
        Vector3 ballPos = ballTransform.position;
        
        // Simply follow the ball position directly
        // The camera will center on the ball and follow it everywhere
        // ClampToBoundsForBall will handle keeping the camera in valid bounds
        return new Vector3(ballPos.x, ballPos.y, cam.transform.position.z);
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
    
    void ClampToBoundsForBall()
    {
        if (!boundsCollider || !ballTransform) return;

        Bounds b = boundsCollider.bounds;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        
        // Get ball position
        Vector3 ballPos = ballTransform.position;
        Vector3 camPos = cam.transform.position;
        
        // Calculate the camera bounds that would show the entire scene
        float minCamX = b.min.x + halfW;
        float maxCamX = b.max.x - halfW;
        float minCamY = b.min.y + halfH;
        float maxCamY = b.max.y - halfH;
        
        // If the ball is near the edges of the scene, allow the camera to go beyond normal bounds
        // to keep the ball visible, but still prevent showing beyond the scene
        
        // Check if ball is near scene edges and adjust camera limits accordingly
        float ballBuffer = 1f; // Distance from edge where we start adjusting camera
        
        // Horizontal adjustments
        if (ballPos.x < b.min.x + ballBuffer)
        {
            // Ball is near left edge - allow camera to move further left
            minCamX = b.min.x;
        }
        else if (ballPos.x > b.max.x - ballBuffer)
        {
            // Ball is near right edge - allow camera to move further right
            maxCamX = b.max.x;
        }
        
        // Vertical adjustments
        if (ballPos.y < b.min.y + ballBuffer)
        {
            // Ball is near bottom edge - allow camera to move further down
            minCamY = b.min.y;
        }
        else if (ballPos.y > b.max.y - ballBuffer)
        {
            // Ball is near top edge - allow camera to move further up
            maxCamY = b.max.y;
        }
        
        // Clamp camera position with adjusted bounds
        camPos.x = Mathf.Clamp(camPos.x, minCamX, maxCamX);
        camPos.y = Mathf.Clamp(camPos.y, minCamY, maxCamY);
        cam.transform.position = camPos;
    }
}
