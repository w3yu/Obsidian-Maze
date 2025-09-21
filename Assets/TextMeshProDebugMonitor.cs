using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TextMeshProDebugMonitor : MonoBehaviour
{
    private Dictionary<int, TextMeshProUGUI> trackedTexts = new Dictionary<int, TextMeshProUGUI>();
    private List<string> destroyedTextNames = new List<string>();
    
    void Awake()
    {
        Debug.Log("[TMPDebugMonitor] Starting TextMeshPro Debug Monitor");
        
        // Find all TextMeshProUGUI components in the scene at startup
        RefreshTextMeshProList();
    }
    
    void Start()
    {
        // Double-check after all Start methods have run
        RefreshTextMeshProList();
    }
    
    void Update()
    {
        // Check for destroyed components every frame
        CheckForDestroyedComponents();
    }
    
    void RefreshTextMeshProList()
    {
        // Clear and rebuild the list
        trackedTexts.Clear();
        
        // Find ALL TextMeshProUGUI components, including inactive ones
        var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
            .Where(t => !string.IsNullOrEmpty(t.gameObject.scene.name)) // Only scene objects, not prefabs
            .ToArray();
        
        Debug.Log($"[TMPDebugMonitor] Found {allTexts.Length} TextMeshProUGUI components in scene");
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                int instanceId = text.GetInstanceID();
                trackedTexts[instanceId] = text;
                
                string path = GetGameObjectPath(text.gameObject);
                Debug.Log($"[TMPDebugMonitor] Tracking TextMeshProUGUI: {path} (ID: {instanceId}, Active: {text.gameObject.activeInHierarchy})");
            }
        }
    }
    
    void CheckForDestroyedComponents()
    {
        List<int> toRemove = new List<int>();
        
        foreach (var kvp in trackedTexts)
        {
            try
            {
                // Try to access the component
                if (kvp.Value == null || ReferenceEquals(kvp.Value, null))
                {
                    Debug.LogWarning($"[TMPDebugMonitor] TextMeshProUGUI with ID {kvp.Key} has been destroyed!");
                    toRemove.Add(kvp.Key);
                    destroyedTextNames.Add($"ID: {kvp.Key} destroyed at frame {Time.frameCount}");
                }
                else
                {
                    // Try to access a property to trigger MissingReferenceException if destroyed
                    var test = kvp.Value.text;
                }
            }
            catch (MissingReferenceException e)
            {
                Debug.LogError($"[TMPDebugMonitor] MissingReferenceException for TextMeshProUGUI ID {kvp.Key}: {e.Message}");
                toRemove.Add(kvp.Key);
                
                // This is likely the problematic component
                Debug.LogError($"[TMPDebugMonitor] *** FOUND THE PROBLEMATIC COMPONENT! ID: {kvp.Key} ***");
                Debug.LogError($"[TMPDebugMonitor] Stack trace: {e.StackTrace}");
            }
        }
        
        // Remove destroyed components from tracking
        foreach (var id in toRemove)
        {
            trackedTexts.Remove(id);
        }
    }
    
    string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    void OnDestroy()
    {
        Debug.Log($"[TMPDebugMonitor] Shutting down. {destroyedTextNames.Count} components were destroyed during session:");
        foreach (var name in destroyedTextNames)
        {
            Debug.Log($"  - {name}");
        }
    }
    
    // Called when a new TextMeshProUGUI is added to the scene
    public void RegisterNewText(TextMeshProUGUI text)
    {
        if (text != null)
        {
            int instanceId = text.GetInstanceID();
            if (!trackedTexts.ContainsKey(instanceId))
            {
                trackedTexts[instanceId] = text;
                string path = GetGameObjectPath(text.gameObject);
                Debug.Log($"[TMPDebugMonitor] New TextMeshProUGUI registered: {path} (ID: {instanceId})");
            }
        }
    }
}
