using UnityEngine;

public class BonusObjectTrigger : MonoBehaviour
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
            
            // Find the PlayerLauncher component and increment shotCount
            PlayerLauncher playerLauncher = FindFirstObjectByType<PlayerLauncher>();
            if (playerLauncher != null)
            {
                playerLauncher.shotCount++;
                Debug.Log("Shot count increased! New count: " + playerLauncher.shotCount);
            }
            else
            {
                Debug.LogWarning("PlayerLauncher not found!");
            }
            
            // Destroy the object after X seconds
            Destroy(gameObject, 1f);
        }
    }
}