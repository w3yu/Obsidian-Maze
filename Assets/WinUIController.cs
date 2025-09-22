using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;

public class WinUIController : MonoBehaviour
{
    [Header("References")]
    public GameObject rootPanel;   // Assign WinPanel or leave empty to use this GO

    [Header("Events")]
    public UnityEvent onNextLevel;
    
    [Header("Button Text References")]
    public TextMeshProUGUI retryButtonText;
    public TextMeshProUGUI nextLevelButtonText;

    CanvasGroup cg;
    private TMPro.TextMeshProUGUI[] cachedTextComponents;

    void Awake()
    {
        Debug.Log($"[WinUIController] Awake called on {gameObject.name}");
        
        if (rootPanel == null) rootPanel = gameObject;

        // Ensure there is a CanvasGroup to control visibility without disabling the object
        cg = rootPanel.GetComponent<CanvasGroup>();
        if (!cg) cg = rootPanel.AddComponent<CanvasGroup>();

        // Cache and log all TextMeshProUGUI children
        cachedTextComponents = rootPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        Debug.Log($"[WinUIController] Found {cachedTextComponents.Length} TextMeshProUGUI components in children");
        foreach (var text in cachedTextComponents)
        {
            Debug.Log($"[WinUIController] Child TextMeshProUGUI: {text.name} (ID: {text.GetInstanceID()})");
        }
        
        // Try to find button text components if not assigned
        if (retryButtonText == null || nextLevelButtonText == null)
        {
            FindButtonTextComponents();
        }
        
        // Update button texts based on current level
        UpdateButtonTextsForLevel();

        // Start hidden regardless of initial inspector state
        rootPanel.SetActive(true); // keep active so lifecycle runs
        HideInstant();
    }
    
    void FindButtonTextComponents()
    {
        foreach (var text in cachedTextComponents)
        {
            if (text == null) continue;
            
            string lowerName = text.name.ToLower();
            
            // Try to find retry button text (could be named "retry", "retrytext", "retrybuttontext", etc.)
            if (retryButtonText == null && (lowerName.Contains("retry") || text.text.ToLower().Contains("retry")))
            {
                retryButtonText = text;
                Debug.Log($"[WinUIController] Found retry button text: {text.name}");
            }
            
            // Try to find next level button text (could be named "next", "nextlevel", "nextleveltext", etc.)
            if (nextLevelButtonText == null && (lowerName.Contains("next") || text.text.ToLower().Contains("next")))
            {
                nextLevelButtonText = text;
                Debug.Log($"[WinUIController] Found next level button text: {text.name}");
            }
        }
    }
    
    void UpdateButtonTextsForLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[WinUIController] Current scene: {sceneName}");
        
        if (sceneName == "level 2")
        {
            // Change button texts for level 2
            if (retryButtonText != null)
            {
                retryButtonText.text = "Tutorial";
                Debug.Log("[WinUIController] Changed retry button to 'Tutorial'");
            }
            
            if (nextLevelButtonText != null)
            {
                nextLevelButtonText.text = "Play Again";
                Debug.Log("[WinUIController] Changed next level button to 'Play Again'");
            }
        }
        // For tutorial level, keep default texts
    }

    public void Show()
    {
        Debug.Log("[WinUIController] Show() called");
        UpdateButtonTextsForLevel(); // Update button texts when showing the win UI
        SetVisible(true);
        Time.timeScale = 0f;
    }

    public void OnRetry()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == "level 2")
        {
            // In level 2, "retry" button goes to Tutorial
            Debug.Log("[WinUIController] OnRetry() called in level 2 - loading Tutorial");
            Time.timeScale = 1f;
            SceneManager.LoadScene(0); // Tutorial_level is at index 0
        }
        else
        {
            // In other levels, reload current scene
            Debug.Log("[WinUIController] OnRetry() called - reloading scene");
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }

    public void OnNextLevel()
    {
        onNextLevel?.Invoke();
        
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == "level 2")
        {
            // In level 2, "Play Again" reloads level 2
            Debug.Log("[WinUI] Play Again clicked in level 2 - reloading level 2");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            // In other levels, go to next level
            Debug.Log("[WinUI] Next Level clicked - loading level 2");
            Time.timeScale = 1f;
            
            // Load the next scene in the build index (level 2 is at index 1)
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            // Check if the next scene exists in build settings
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("[WinUI] No next level available - reloading current scene");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    // --- helpers ---
    void SetVisible(bool visible)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
        
        // Also disable raycast targets on all TextMeshProUGUI components when hidden
        if (cachedTextComponents != null)
        {
            foreach (var text in cachedTextComponents)
            {
                if (text != null && !ReferenceEquals(text, null))
                {
                    text.raycastTarget = visible;
                }
            }
        }
    }

    void HideInstant() => SetVisible(false);
    
    void OnDestroy()
    {
        Debug.Log($"[WinUIController] OnDestroy called on {gameObject.name}");
        
        // Ensure all text components have raycasting disabled before destruction
        if (cachedTextComponents != null)
        {
            foreach (var text in cachedTextComponents)
            {
                if (text != null && !ReferenceEquals(text, null))
                {
                    text.raycastTarget = false;
                }
            }
        }
        
        // Check for any remaining TextMeshProUGUI components
        var textComponents = rootPanel?.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        if (textComponents != null)
        {
            Debug.Log($"[WinUIController] {textComponents.Length} TextMeshProUGUI components still present on destroy");
        }
    }
}
