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

    void Awake()
    {
        if (rootPanel == null) rootPanel = gameObject;

        // Ensure there is a CanvasGroup to control visibility without disabling the object
        cg = rootPanel.GetComponent<CanvasGroup>();
        if (!cg) cg = rootPanel.AddComponent<CanvasGroup>();

        // Start hidden regardless of initial inspector state
        rootPanel.SetActive(true); // keep active so lifecycle runs
        HideInstant();
    }

    public void Show()
    {
        SetVisible(true);
        Time.timeScale = 0f;
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void OnNextLevel()
    {
        onNextLevel?.Invoke();
        Debug.Log("[WinUI] Next Level clicked – wire up 'onNextLevel' in Inspector.");
    }

    // --- helpers ---
    void SetVisible(bool visible)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    void HideInstant() => SetVisible(false);
}
