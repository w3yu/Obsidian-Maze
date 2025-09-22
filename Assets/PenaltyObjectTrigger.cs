using UnityEngine;

public class PenaltyObjectTrigger : MonoBehaviour
{
    private Animator animator;
    private bool hasTriggered = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball") && !hasTriggered)
        {
            hasTriggered = true;
            animator.SetTrigger("Activate");
            
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
