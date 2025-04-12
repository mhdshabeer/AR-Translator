using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages the translation process and coordinates between text recognition and display
/// </summary>
public class TranslationManager : MonoBehaviour
{
    [SerializeField] private TextRecognizer textRecognizer;
    [SerializeField] private TextOverlayManager textOverlayManager;
    
    private Language sourceLanguage = Language.English;
    private Language targetLanguage = Language.Spanish;
    
    private TranslationAPI translationAPI;
    private Dictionary<string, string> translationCache = new Dictionary<string, string>();
    
    public static TranslationManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize translation API
        translationAPI = GetComponent<TranslationAPI>();
    }

    private void OnEnable()
    {
        if (textRecognizer != null)
        {
            textRecognizer.OnTextRecognized += HandleTextRecognized;
        }
    }

    private void OnDisable()
    {
        if (textRecognizer != null)
        {
            textRecognizer.OnTextRecognized -= HandleTextRecognized;
        }
    }

    private void HandleTextRecognized(string recognizedText, Rect textRect)
    {
        // Don't translate empty text
        if (string.IsNullOrWhiteSpace(recognizedText))
        {
            return;
        }
        
        // Check if we already have this translation cached
        if (translationCache.TryGetValue(GetCacheKey(recognizedText), out string cachedTranslation))
        {
            DisplayTranslation(recognizedText, cachedTranslation, textRect);
            return;
        }
        
        // If not in cache, request a new translation
        StartCoroutine(TranslateText(recognizedText, textRect));
    }

    private IEnumerator TranslateText(string text, Rect textRect)
    {
        // Request translation from the API
        string sourceLangCode = LanguageExtensions.GetLanguageCode(sourceLanguage);
        string targetLangCode = LanguageExtensions.GetLanguageCode(targetLanguage);
        
        bool translationComplete = false;
        string translatedText = string.Empty;
        
        translationAPI.Translate(text, sourceLangCode, targetLangCode, (success, result) => {
            translationComplete = true;
            if (success)
            {
                translatedText = result;
                
                // Cache the translation
                string cacheKey = GetCacheKey(text);
                translationCache[cacheKey] = translatedText;
                
                // Display the translation
                DisplayTranslation(text, translatedText, textRect);
            }
            else
            {
                Debug.LogWarning($"Translation failed for text: {text}");
            }
        });
        
        // Wait for translation to complete with a timeout
        float timer = 0f;
        while (!translationComplete && timer < 5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (!translationComplete)
        {
            Debug.LogError("Translation request timed out");
        }
    }

    private void DisplayTranslation(string originalText, string translatedText, Rect textRect)
    {
        if (textOverlayManager != null)
        {
            textOverlayManager.ShowTranslation(originalText, translatedText, textRect);
        }
    }

    private string GetCacheKey(string text)
    {
        // Create a unique key for the cache based on text and language pair
        return $"{text}_{sourceLanguage}_{targetLanguage}";
    }

    /// <summary>
    /// Sets the source language for translation
    /// </summary>
    public void SetSourceLanguage(Language language)
    {
        if (sourceLanguage != language)
        {
            sourceLanguage = language;
            translationCache.Clear(); // Clear cache when language changes
            Debug.Log($"Source language set to: {language}");
        }
    }

    /// <summary>
    /// Sets the target language for translation
    /// </summary>
    public void SetTargetLanguage(Language language)
    {
        if (targetLanguage != language)
        {
            targetLanguage = language;
            translationCache.Clear(); // Clear cache when language changes
            Debug.Log($"Target language set to: {language}");
        }
    }

    /// <summary>
    /// Swaps source and target languages
    /// </summary>
    public void SwapLanguages()
    {
        Language temp = sourceLanguage;
        sourceLanguage = targetLanguage;
        targetLanguage = temp;
        
        translationCache.Clear(); // Clear cache when languages swap
        Debug.Log("Languages swapped");
        
        // Update UI
        UIManager.Instance.UpdateLanguageDisplay(sourceLanguage, targetLanguage);
    }
    
    /// <summary>
    /// Gets the current source language
    /// </summary>
    public Language GetSourceLanguage()
    {
        return sourceLanguage;
    }
    
    /// <summary>
    /// Gets the current target language
    /// </summary>
    public Language GetTargetLanguage()
    {
        return targetLanguage;
    }
}
