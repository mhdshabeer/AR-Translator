using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum representing supported languages
/// </summary>
public enum Language
{
    English,
    Spanish,
    French,
    German,
    Italian,
    Portuguese,
    Russian,
    Chinese,
    Japanese,
    Korean,
    Arabic
}

/// <summary>
/// Extension methods for the Language enum
/// </summary>
public static class LanguageExtensions
{
    /// <summary>
    /// Gets the language code used by translation APIs
    /// </summary>
    public static string GetLanguageCode(this Language language)
    {
        switch (language)
        {
            case Language.English: return "en";
            case Language.Spanish: return "es";
            case Language.French: return "fr";
            case Language.German: return "de";
            case Language.Italian: return "it";
            case Language.Portuguese: return "pt";
            case Language.Russian: return "ru";
            case Language.Chinese: return "zh";
            case Language.Japanese: return "ja";
            case Language.Korean: return "ko";
            case Language.Arabic: return "ar";
            default: return "en";
        }
    }
    
    /// <summary>
    /// Gets a language from a language code
    /// </summary>
    public static Language GetLanguageFromCode(string code)
    {
        switch (code.ToLower())
        {
            case "en": return Language.English;
            case "es": return Language.Spanish;
            case "fr": return Language.French;
            case "de": return Language.German;
            case "it": return Language.Italian;
            case "pt": return Language.Portuguese;
            case "ru": return Language.Russian;
            case "zh": return Language.Chinese;
            case "ja": return Language.Japanese;
            case "ko": return Language.Korean;
            case "ar": return Language.Arabic;
            default: return Language.English;
        }
    }
    
    /// <summary>
    /// Gets a list of all available languages
    /// </summary>
    public static List<Language> GetAvailableLanguages()
    {
        return new List<Language>
        {
            Language.English,
            Language.Spanish,
            Language.French,
            Language.German,
            Language.Italian,
            Language.Portuguese,
            Language.Russian,
            Language.Chinese,
            Language.Japanese,
            Language.Korean,
            Language.Arabic
        };
    }
}
