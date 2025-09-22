using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PenaltyObjectTrigger : MonoBehaviour
{
    private Animator animator;
    private bool hasTriggered = false;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip penaltySound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.5f;
    private AudioSource audioSource;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Add "-1" text label if in tutorial scene
        if (SceneManager.GetActiveScene().name == "Tutorial_level")
        {
            CreateTextLabel("-1");
        }
    }
    
    private void CreateTextLabel(string text)
    {
        // Create a new GameObject for the text
        GameObject textObj = new GameObject("PenaltyLabel");
        textObj.transform.SetParent(transform, false);
        
        // Position the text above the object
        textObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        // Add TextMeshPro component
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 6;
        tmp.color = Color.red;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 10; // Ensure it appears above other sprites
        
        // Make the text face the camera
        tmp.rectTransform.sizeDelta = new Vector2(2, 1);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball") && !hasTriggered)
        {
            hasTriggered = true;
            animator.SetTrigger("Activate");
            
            // Play sound effect
            if (penaltySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(penaltySound, soundVolume);
            }
            
            // Find the PlayerLauncher component and DECREMENT shotCount
            PlayerLauncher playerLauncher = FindFirstObjectByType<PlayerLauncher>();
            if (playerLauncher != null)
            {
                playerLauncher.shotCount--;
                playerLauncher.penaltiesHit++;
                Debug.Log("Penalty hit! Shot count: " + playerLauncher.shotCount + 
                         ", Total penalties: " + playerLauncher.penaltiesHit);
            }
            else
            {
                Debug.LogWarning("PlayerLauncher not found!");
            }
            
            // Destroy the object after animation
            Destroy(gameObject, 1f);
        }
    }
}
