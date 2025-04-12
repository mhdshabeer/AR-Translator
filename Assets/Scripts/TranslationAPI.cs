using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles API calls to translation services
/// </summary>
public class TranslationAPI : MonoBehaviour
{
    [SerializeField] private bool useLibreTranslate = true; // Set to false to use Google Translate
    
    [SerializeField] private string libreTranslateUrl = "https://libretranslate.com/translate";
    [SerializeField] private string googleTranslateBaseUrl = "https://translation.googleapis.com/language/translate/v2";
    
    [SerializeField] private string apiKey = ""; // Only needed for Google Translate
    
    private void Awake()
    {
        // Try to get API key from environment variable if it's not set
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("TRANSLATE_API_KEY") ?? "";
        }
    }

    /// <summary>
    /// Translates text from source language to target language
    /// </summary>
    /// <param name="text">The text to translate</param>
    /// <param name="sourceLanguage">The source language code</param>
    /// <param name="targetLanguage">The target language code</param>
    /// <param name="callback">Callback with success status and translated text</param>
    public void Translate(string text, string sourceLanguage, string targetLanguage, Action<bool, string> callback)
    {
        if (useLibreTranslate)
        {
            StartCoroutine(TranslateWithLibreTranslate(text, sourceLanguage, targetLanguage, callback));
        }
        else
        {
            StartCoroutine(TranslateWithGoogleTranslate(text, sourceLanguage, targetLanguage, callback));
        }
    }

    private IEnumerator TranslateWithLibreTranslate(string text, string sourceLanguage, string targetLanguage, Action<bool, string> callback)
    {
        // Create form data for the request
        WWWForm form = new WWWForm();
        form.AddField("q", text);
        form.AddField("source", sourceLanguage);
        form.AddField("target", targetLanguage);
        form.AddField("format", "text");
        
        // Create and send the request
        using (UnityWebRequest request = UnityWebRequest.Post(libreTranslateUrl, form))
        {
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Translation API error: {request.error}");
                callback(false, "");
                yield break;
            }
            
            // Parse the response
            string jsonResponse = request.downloadHandler.text;
            LibreTranslateResponse response = JsonUtility.FromJson<LibreTranslateResponse>(jsonResponse);
            
            if (response != null && !string.IsNullOrEmpty(response.translatedText))
            {
                callback(true, response.translatedText);
            }
            else
            {
                Debug.LogError("Failed to parse translation response");
                callback(false, "");
            }
        }
    }

    private IEnumerator TranslateWithGoogleTranslate(string text, string sourceLanguage, string targetLanguage, Action<bool, string> callback)
    {
        // Construct the URL
        string url = $"{googleTranslateBaseUrl}?q={UnityWebRequest.EscapeURL(text)}&source={sourceLanguage}&target={targetLanguage}&key={apiKey}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Google Translate API error: {request.error}");
                callback(false, "");
                yield break;
            }
            
            // Parse the response
            string jsonResponse = request.downloadHandler.text;
            GoogleTranslateResponse response = JsonUtility.FromJson<GoogleTranslateResponse>(jsonResponse);
            
            if (response != null && response.data != null && 
                response.data.translations != null && response.data.translations.Length > 0)
            {
                callback(true, response.data.translations[0].translatedText);
            }
            else
            {
                Debug.LogError("Failed to parse Google translation response");
                callback(false, "");
            }
        }
    }

    // Response classes for JSON deserialization
    [Serializable]
    private class LibreTranslateResponse
    {
        public string translatedText;
    }

    [Serializable]
    private class GoogleTranslateResponse
    {
        public GoogleTranslateData data;
    }

    [Serializable]
    private class GoogleTranslateData
    {
        public GoogleTranslation[] translations;
    }

    [Serializable]
    private class GoogleTranslation
    {
        public string translatedText;
    }
}
