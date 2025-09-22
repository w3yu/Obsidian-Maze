using UnityEngine;

public class ShotModifierObject : MonoBehaviour
{
    [Header("Shot Modification")]
    [Tooltip("Amount to modify shots by (positive adds, negative removes)")]
    public int shotModificationAmount = 1;
    
    [Header("Animation")]
    [Tooltip("Use Unity Animator for hit animation")]
    public bool useAnimator = true;
    
    [Tooltip("Trigger parameter name in Animator")]
    public string hitAnimationTrigger = "Hit";
    
    [Tooltip("Duration to wait before destroying (should match animation length)")]
    public float animationDuration = 1.0f;
    
    [Header("Legacy Visual Feedback (if not using Animator)")]
    [Tooltip("Color to flash when hit")]
    public Color hitFlashColor = Color.white;
    
    [Tooltip("Duration of the hit flash")]
    public float flashDuration = 0.2f;
    
    [Tooltip("Scale multiplier when hit")]
    public float hitScaleMultiplier = 1.2f;
    
    [Tooltip("Duration of scale animation")]
    public float scaleAnimationDuration = 0.3f;
    
    [Header("Audio")]
    [Tooltip("Sound to play when hit (optional)")]
    public AudioClip hitSound;
    
    [Tooltip("Volume for the hit sound")]
    [Range(0f, 1f)]
    public float hitSoundVolume = 0.5f;
    
    // References
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private PlayerLauncher playerLauncher;
    private AudioSource audioSource;
    private Animator animator;
    private Collider2D objectCollider;
    
    // Animation state
    private bool isAnimating = false;
    private float animationTimer = 0f;
    private bool hasBeenHit = false;
    
    void Start()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        originalScale = transform.localScale;
        
        // Get animator if using animations
        if (useAnimator)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[ShotModifierObject] useAnimator is true but no Animator component found on {gameObject.name}");
                useAnimator = false;
            }
        }
        
        // Get collider
        objectCollider = GetComponent<Collider2D>();
        
        // Find PlayerLauncher in the scene
        playerLauncher = FindFirstObjectByType<PlayerLauncher>();
        if (playerLauncher == null)
        {
            Debug.LogError($"[ShotModifierObject] Could not find PlayerLauncher in the scene!");
        }
        
        // Setup audio if needed
        if (hitSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }
    
    void Update()
    {
        // Handle legacy visual animations (when not using Animator)
        if (isAnimating && !useAnimator)
        {
            animationTimer += Time.deltaTime;
            
            // Flash animation
            if (spriteRenderer != null && animationTimer <= flashDuration)
            {
                float flashProgress = animationTimer / flashDuration;
                spriteRenderer.color = Color.Lerp(hitFlashColor, originalColor, flashProgress);
            }
            
            // Scale animation
            if (animationTimer <= scaleAnimationDuration)
            {
                float scaleProgress = animationTimer / scaleAnimationDuration;
                float currentScale = Mathf.Lerp(hitScaleMultiplier, 1f, scaleProgress);
                transform.localScale = originalScale * currentScale;
            }
            
            // End animation
            if (animationTimer >= Mathf.Max(flashDuration, scaleAnimationDuration))
            {
                isAnimating = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
                transform.localScale = originalScale;
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the ball
        if (collision.gameObject.GetComponent<BilliardBounce2D>() == null)
        {
            return; // Not the ball
        }
        
        // Prevent multiple hits
        if (hasBeenHit)
        {
            return;
        }
        
        hasBeenHit = true;
        
        // Disable collider to prevent further collisions
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }
        
        // Modify shot count
        if (playerLauncher != null)
        {
            playerLauncher.shotCount += shotModificationAmount;
            
            // Ensure shot count doesn't go below 0
            if (playerLauncher.shotCount < 0)
            {
                playerLauncher.shotCount = 0;
            }
            
            // Log the modification for debugging
            string action = shotModificationAmount > 0 ? "added" : "removed";
            Debug.Log($"[ShotModifierObject] {Mathf.Abs(shotModificationAmount)} shot(s) {action}. Total shots: {playerLauncher.shotCount}");
            
            // Update UI immediately
            ShotCounterUI shotCounterUI = FindFirstObjectByType<ShotCounterUI>();
            if (shotCounterUI != null)
            {
                shotCounterUI.RefreshDisplay();
            }
        }
        
        // Play sound effect
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, hitSoundVolume);
        }
        
        // Handle animation and destruction
        if (useAnimator && animator != null)
        {
            // Trigger animation
            animator.SetTrigger(hitAnimationTrigger);
            
            // Destroy after animation completes
            StartCoroutine(DestroyAfterAnimation());
        }
        else
        {
            // Use legacy visual feedback
            if (spriteRenderer != null && flashDuration > 0)
            {
                StartCoroutine(DestroyWithFlash());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    System.Collections.IEnumerator DestroyWithFlash()
    {
        // Flash before destroying
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitFlashColor;
        }
        
        // Scale up briefly
        transform.localScale = originalScale * hitScaleMultiplier;
        
        yield return new WaitForSeconds(flashDuration);
        
        Destroy(gameObject);
    }
    
    System.Collections.IEnumerator DestroyAfterAnimation()
    {
        // Wait for the animation to complete
        yield return new WaitForSeconds(animationDuration);
        
        // Destroy the object
        Destroy(gameObject);
    }
}
