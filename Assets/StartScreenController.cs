using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartScreenController : MonoBehaviour
{
    [Header("Text References")]
    public GameObject startScreenPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionsText;
    public Button startButton;
    
    [Header("Visual Instruction Images")]
    [Tooltip("Arrow images for controls - will be rotated to show directions")]
    public Image arrowUpImage;
    public Image arrowDownImage;
    public Image arrowLeftImage;
    public Image arrowRightImage;
    
    [Tooltip("Item sprites to show what to collect/avoid")]
    public Image bonusItemImage;
    public Image penaltyItemImage;
    
    [Header("Game References")]
    public PlayerLauncher playerLauncher;
    
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        // Use this GameObject if no panel is specified
        if (startScreenPanel == null)
            startScreenPanel = gameObject;
        
        // Ensure we have a CanvasGroup for visibility control
        canvasGroup = startScreenPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = startScreenPanel.AddComponent<CanvasGroup>();
        
        // Find PlayerLauncher if not assigned
        if (playerLauncher == null)
            playerLauncher = FindObjectOfType<PlayerLauncher>();
        
        // Set up the button listener
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        
        // Setup visual elements
        SetupVisualElements();
        
        // Show the start screen and pause the game
        ShowStartScreen();
    }
    
    void Start()
    {
        // Ensure the start screen is shown when the game starts
        ShowStartScreen();
    }
    
    // Text content is now set directly in the Unity Editor Inspector
    // This allows for easier customization without modifying code
    
    void SetupVisualElements()
    {
        // Setup arrow rotations if they exist
        if (arrowUpImage != null)
        {
            arrowUpImage.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        
        if (arrowDownImage != null)
        {
            arrowDownImage.transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        
        if (arrowLeftImage != null)
        {
            arrowLeftImage.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        
        if (arrowRightImage != null)
        {
            arrowRightImage.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
        // Bonus and penalty images don't need rotation, just ensure they're visible
        if (bonusItemImage != null)
        {
            bonusItemImage.preserveAspect = true;
        }
        
        if (penaltyItemImage != null)
        {
            penaltyItemImage.preserveAspect = true;
        }
    }
    
    
    void ShowStartScreen()
    {
        // Show the panel
        SetVisible(true);
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Disable player controls
        if (playerLauncher != null)
        {
            playerLauncher.enabled = false;
        }
        
        Debug.Log("[StartScreenController] Start screen shown, game paused");
    }
    
    void OnStartButtonClicked()
    {
        Debug.Log("[StartScreenController] Start button clicked");
        
        // Hide the start screen
        SetVisible(false);
        
        // Unpause the game
        Time.timeScale = 1f;
        
        // Enable player controls
        if (playerLauncher != null)
        {
            playerLauncher.enabled = true;
            // Reset shot count to ensure fresh start
            playerLauncher.ResetShotCount();
        }
        
        Debug.Log("[StartScreenController] Game started!");
    }
    
    void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            startScreenPanel.SetActive(visible);
        }
    }
    
    // Public method to show the start screen again if needed
    public void Show()
    {
        ShowStartScreen();
    }
    
    void OnDestroy()
    {
        // Clean up button listener
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        
        // Ensure time scale is reset
        Time.timeScale = 1f;
    }
}
