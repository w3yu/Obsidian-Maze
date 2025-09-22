using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class WinUIController : MonoBehaviour
{
    [Header("References")]
    public GameObject rootPanel;   // Assign WinPanel or leave empty to use this GO

    [Header("Events")]
    public UnityEvent onNextLevel;

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

        // Start hidden regardless of initial inspector state
        rootPanel.SetActive(true); // keep active so lifecycle runs
        HideInstant();
    }

    public void Show()
    {
        Debug.Log("[WinUIController] Show() called");
        SetVisible(true);
        Time.timeScale = 0f;
    }

    public void OnRetry()
    {
        Debug.Log("[WinUIController] OnRetry() called - reloading scene");
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void OnNextLevel()
    {
        onNextLevel?.Invoke();
        Debug.Log("[WinUI] Next Level clicked – wire up 'onNextLevel' in Inspector.");
        var scene = SceneManager.GetActiveScene();
        int nextIndex = scene.buildIndex + 1;

        // 如果还有下一关，就加载
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextIndex);
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
