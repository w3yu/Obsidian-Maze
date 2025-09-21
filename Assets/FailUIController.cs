using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;

public class FailUIController : MonoBehaviour
{
    [Header("References")]
    public GameObject rootPanel;   // Assign FailPanel or leave empty to use this GO
    public TMPro.TextMeshProUGUI failText;  // Reference to fail message text
    public Button retryButton;              // Reference to retry button

    [Header("Events")]
    public UnityEvent onNextLevel;

    CanvasGroup cg;
    private TMPro.TextMeshProUGUI[] cachedTextComponents;

    void Awake()
    {
        Debug.Log($"[FailUIController] Awake called on {gameObject.name}");
        
        if (rootPanel == null) rootPanel = gameObject;

        // Ensure there is a CanvasGroup to control visibility without disabling the object
        cg = rootPanel.GetComponent<CanvasGroup>();
        if (!cg) cg = rootPanel.AddComponent<CanvasGroup>();

        // Create UI structure if components are missing
        SetupFailUI();

        // Cache and log all TextMeshProUGUI children
        cachedTextComponents = rootPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        Debug.Log($"[FailUIController] Found {cachedTextComponents.Length} TextMeshProUGUI components in children");
        foreach (var text in cachedTextComponents)
        {
            Debug.Log($"[FailUIController] Child TextMeshProUGUI: {text.name} (ID: {text.GetInstanceID()})");
        }

        // Start hidden regardless of initial inspector state
        rootPanel.SetActive(true); // keep active so lifecycle runs
        HideInstant();
    }

    void SetupFailUI()
    {
        // Check if we need to create the retry button
        if (retryButton == null)
        {
            // Look for existing button first
            Button[] buttons = rootPanel.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.name.ToLower().Contains("retry"))
                {
                    retryButton = btn;
                    break;
                }
            }

            // If no retry button exists, create one
            if (retryButton == null)
            {
                Debug.Log("[FailUIController] Creating Retry button programmatically");
                
                // Create retry button GameObject
                GameObject retryButtonGO = new GameObject("RetryButton");
                retryButtonGO.transform.SetParent(rootPanel.transform, false);
                
                // Add RectTransform and position it
                RectTransform rectTransform = retryButtonGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.3f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
                rectTransform.anchoredPosition = new Vector2(0, 0);
                rectTransform.sizeDelta = new Vector2(160, 40);
                
                // Add Image component for button background
                Image buttonImage = retryButtonGO.AddComponent<Image>();
                buttonImage.color = new Color(1f, 1f, 1f, 0.9f);
                
                // Add Button component
                retryButton = retryButtonGO.AddComponent<Button>();
                
                // Create text for button
                GameObject buttonTextGO = new GameObject("Text");
                buttonTextGO.transform.SetParent(retryButtonGO.transform, false);
                
                RectTransform textRect = buttonTextGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                TMPro.TextMeshProUGUI buttonText = buttonTextGO.AddComponent<TMPro.TextMeshProUGUI>();
                buttonText.text = "Retry";
                buttonText.fontSize = 24;
                buttonText.color = Color.black;
                buttonText.alignment = TMPro.TextAlignmentOptions.Center;
            }
        }

        // Connect the retry button to OnRetry method
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetry);
        }

        // Setup fail text if not assigned
        if (failText == null)
        {
            failText = rootPanel.GetComponent<TMPro.TextMeshProUGUI>();
            if (failText != null && failText.text.ToLower() != "fail!")
            {
                failText.text = "Fail!";
            }
        }
    }

    public void Show()
    {
        Debug.Log("[FailUIController] Show() called");
        SetVisible(true);
        Time.timeScale = 0f;
    }

    public void OnRetry()
    {
        Debug.Log("[FailUIController] OnRetry() called - reloading scene");
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
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
        Debug.Log($"[FailUIController] OnDestroy called on {gameObject.name}");
        
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
            Debug.Log($"[FailUIController] {textComponents.Length} TextMeshProUGUI components still present on destroy");
        }
    }
}
