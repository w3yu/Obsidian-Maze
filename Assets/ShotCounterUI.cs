using UnityEngine;
using TMPro;

public class ShotCounterUI : MonoBehaviour
{
    [Header("References")]
    public PlayerLauncher playerLauncher;
    public TextMeshProUGUI shotCountText;

    [Header("Display Settings")]
    public string displayFormat = "Shots Remaining: {0}\nBonuses: {1}\nPenalties: {2}\nScore: {3}";

    void Start()
    {
        Debug.Log($"[ShotCounterUI] Start called on {gameObject.name}");
        
        // Ensure the display format is correct
        displayFormat = "Shots Remaining: {0}\nBonuses: {1}\nPenalties: {2}\nScore: {3}";
        
        // Try to find PlayerLauncher if not assigned
        if (playerLauncher == null)
        {
            playerLauncher = FindFirstObjectByType<PlayerLauncher>();
            Debug.Log($"[ShotCounterUI] PlayerLauncher auto-found: {playerLauncher != null}");
        }

        // Try to find TextMeshPro component if not assigned
        if (shotCountText == null)
        {
            shotCountText = GetComponent<TextMeshProUGUI>();
            Debug.Log($"[ShotCounterUI] TextMeshProUGUI auto-found: {shotCountText != null}");
        }
        
        if (shotCountText != null)
        {
            Debug.Log($"[ShotCounterUI] TextMeshProUGUI Instance ID: {shotCountText.GetInstanceID()}");
        }

        // Initialize display
        UpdateDisplay();
    }

    void Update()
    {
        // Only update if the component still exists and hasn't been destroyed
        if (shotCountText != null && !ReferenceEquals(shotCountText, null))
        {
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        // Use Unity's proper null checking pattern to handle destroyed objects
        if (!ReferenceEquals(playerLauncher, null) && playerLauncher != null && 
            !ReferenceEquals(shotCountText, null) && shotCountText != null)
        {
            try
            {
                int score = playerLauncher.shotCount * 100;
                shotCountText.text = string.Format(displayFormat, 
                    playerLauncher.shotCount,
                    playerLauncher.bonusesCollected,
                    playerLauncher.penaltiesHit,
                    score);
            }
            catch (MissingReferenceException e)
            {
                // Handle the case where the component was destroyed between the check and the access
                Debug.LogError($"[ShotCounterUI] MissingReferenceException in UpdateDisplay: {e.Message}");
                Debug.LogError($"[ShotCounterUI] TextMeshProUGUI Instance ID was: {shotCountText?.GetInstanceID() ?? -1}");
                shotCountText = null;
            }
        }
    }

    /// <summary>
    /// Manually refresh the display. Call this if you want to ensure the UI updates immediately.
    /// </summary>
    public void RefreshDisplay()
    {
        Debug.Log("[ShotCounterUI] RefreshDisplay called");
        UpdateDisplay();
    }
    
    void OnDestroy()
    {
        Debug.Log($"[ShotCounterUI] OnDestroy called on {gameObject.name}");
        if (shotCountText != null)
        {
            Debug.Log($"[ShotCounterUI] TextMeshProUGUI Instance ID on destroy: {shotCountText.GetInstanceID()}");
        }
    }
}
