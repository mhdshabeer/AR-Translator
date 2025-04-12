using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the display of translated text overlays in the AR scene
/// </summary>
public class TextOverlayManager : MonoBehaviour
{
    [SerializeField] private GameObject textOverlayPrefab;
    [SerializeField] private Transform overlayContainer;
    [SerializeField] private float overlayDuration = 5f; // How long each overlay stays visible
    
    private Dictionary<string, TextOverlay> activeOverlays = new Dictionary<string, TextOverlay>();
    
    private Camera arCamera;
    
    private void Start()
    {
        arCamera = ARSessionManager.Instance.ARCamera;
        
        if (overlayContainer == null)
        {
            overlayContainer = transform;
        }
    }

    /// <summary>
    /// Shows a translation overlay at the specified screen position
    /// </summary>
    public void ShowTranslation(string originalText, string translatedText, Rect screenRect)
    {
        // Convert normalized rect to screen coordinates
        Rect screenPositionRect = new Rect(
            screenRect.x * Screen.width,
            screenRect.y * Screen.height,
            screenRect.width * Screen.width,
            screenRect.height * Screen.height
        );
        
        // Check if we already have an overlay for this text
        string overlayKey = GenerateOverlayKey(originalText, screenPositionRect);
        
        if (activeOverlays.TryGetValue(overlayKey, out TextOverlay existingOverlay))
        {
            // Update existing overlay
            existingOverlay.UpdateText(translatedText);
            existingOverlay.ResetTimer();
        }
        else
        {
            // Create new overlay
            GameObject overlayObj = Instantiate(textOverlayPrefab, overlayContainer);
            TextOverlay newOverlay = overlayObj.GetComponent<TextOverlay>();
            
            if (newOverlay != null)
            {
                newOverlay.Initialize(originalText, translatedText, screenPositionRect, overlayDuration);
                newOverlay.OnOverlayExpired += () => RemoveOverlay(overlayKey);
                
                activeOverlays.Add(overlayKey, newOverlay);
            }
        }
    }

    private void RemoveOverlay(string key)
    {
        if (activeOverlays.TryGetValue(key, out TextOverlay overlay))
        {
            Destroy(overlay.gameObject);
            activeOverlays.Remove(key);
        }
    }

    private string GenerateOverlayKey(string text, Rect position)
    {
        // Create a unique key based on text and approximate position
        return $"{text}_{Mathf.RoundToInt(position.x / 50)}_{Mathf.RoundToInt(position.y / 50)}";
    }

    /// <summary>
    /// Clears all active overlays
    /// </summary>
    public void ClearAllOverlays()
    {
        foreach (var overlay in activeOverlays.Values)
        {
            Destroy(overlay.gameObject);
        }
        
        activeOverlays.Clear();
    }
}

/// <summary>
/// Component for individual text overlays
/// </summary>
public class TextOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI translatedTextElement;
    [SerializeField] private RectTransform backgroundRect;
    
    private float duration;
    private float timer;
    
    public event System.Action OnOverlayExpired;

    public void Initialize(string originalText, string translatedText, Rect screenPosition, float displayDuration)
    {
        translatedTextElement.text = translatedText;
        duration = displayDuration;
        timer = 0f;
        
        // Position the overlay
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = new Vector3(
            screenPosition.x + screenPosition.width / 2,
            screenPosition.y - screenPosition.height / 2,
            0
        );
        
        // Size the overlay
        rectTransform.sizeDelta = new Vector2(
            screenPosition.width * 1.1f, // Slightly wider than the original text
            screenPosition.height * 1.5f  // Taller to accommodate translated text
        );
        
        // Make sure the background fits the text
        if (backgroundRect != null)
        {
            StartCoroutine(AdjustBackgroundSize());
        }
    }

    private IEnumerator AdjustBackgroundSize()
    {
        // Wait a frame for TextMeshPro to update its layout
        yield return null;
        
        // Adjust background to fit the text content with some padding
        if (translatedTextElement != null)
        {
            Vector2 textSize = translatedTextElement.GetPreferredValues();
            backgroundRect.sizeDelta = textSize + new Vector2(20f, 10f); // Add padding
        }
    }

    public void UpdateText(string newText)
    {
        translatedTextElement.text = newText;
        
        // Adjust background size for new text
        if (backgroundRect != null)
        {
            StartCoroutine(AdjustBackgroundSize());
        }
    }

    public void ResetTimer()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        // Calculate fade out in the last second
        if (timer > duration - 1f)
        {
            float alpha = 1f - (timer - (duration - 1f));
            SetAlpha(alpha);
        }
        
        // Remove when duration is reached
        if (timer >= duration)
        {
            OnOverlayExpired?.Invoke();
        }
    }

    private void SetAlpha(float alpha)
    {
        // Set alpha on the text and any other UI elements
        Color textColor = translatedTextElement.color;
        textColor.a = alpha;
        translatedTextElement.color = textColor;
        
        // Also set alpha on the background if applicable
        if (backgroundRect != null)
        {
            CanvasGroup canvasGroup = backgroundRect.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }
    }
}
