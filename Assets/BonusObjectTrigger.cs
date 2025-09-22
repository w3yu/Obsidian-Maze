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
                playerLauncher.bonusesCollected++;
                Debug.Log("Bonus collected! Shot count: " + playerLauncher.shotCount + 
                         ", Total bonuses: " + playerLauncher.bonusesCollected);
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
