using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the user interface for the translation app
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject languageSelectionPanel;
    [SerializeField] private GameObject errorPanel;
    
    [Header("Language UI")]
    [SerializeField] private TextMeshProUGUI sourceLanguageText;
    [SerializeField] private TextMeshProUGUI targetLanguageText;
    [SerializeField] private Button swapLanguagesButton;
    [SerializeField] private Button openLanguageSelectionButton;
    
    [Header("Language Selection")]
    [SerializeField] private Transform sourceLanguageContainer;
    [SerializeField] private Transform targetLanguageContainer;
    [SerializeField] private GameObject languageButtonPrefab;
    
    [Header("Error Messages")]
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private Button dismissErrorButton;
    
    private bool isSelectingSourceLanguage = true;
    private List<Language> availableLanguages;
    
    public static UIManager Instance { get; private set; }

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
        
        // Set up language list
        availableLanguages = LanguageExtensions.GetAvailableLanguages();
    }

    private void Start()
    {
        // Initialize UI state
        ShowMainPanel();
        
        // Set up button listeners
        if (swapLanguagesButton != null)
        {
            swapLanguagesButton.onClick.AddListener(OnSwapLanguagesClicked);
        }
        
        if (openLanguageSelectionButton != null)
        {
            openLanguageSelectionButton.onClick.AddListener(() => ShowLanguageSelectionPanel(true));
        }
        
        if (dismissErrorButton != null)
        {
            dismissErrorButton.onClick.AddListener(DismissError);
        }
        
        // Update language display with initial values
        UpdateLanguageDisplay(
            TranslationManager.Instance.GetSourceLanguage(),
            TranslationManager.Instance.GetTargetLanguage()
        );
    }

    public void UpdateLanguageDisplay(Language sourceLanguage, Language targetLanguage)
    {
        if (sourceLanguageText != null)
        {
            sourceLanguageText.text = sourceLanguage.ToString();
        }
        
        if (targetLanguageText != null)
        {
            targetLanguageText.text = targetLanguage.ToString();
        }
    }

    public void ShowMainPanel()
    {
        SetActivePanel(mainPanel);
    }

    public void ShowLanguageSelectionPanel(bool selectingSource)
    {
        isSelectingSourceLanguage = selectingSource;
        SetActivePanel(languageSelectionPanel);
        
        // Populate language options
        PopulateLanguageOptions();
    }

    public void ShowARNotSupportedMessage()
    {
        ShowErrorMessage("AR is not supported on this device.");
    }

    public void ShowErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
        }
        
        SetActivePanel(errorPanel);
    }

    private void DismissError()
    {
        ShowMainPanel();
    }

    private void SetActivePanel(GameObject panel)
    {
        if (mainPanel != null) mainPanel.SetActive(panel == mainPanel);
        if (languageSelectionPanel != null) languageSelectionPanel.SetActive(panel == languageSelectionPanel);
        if (errorPanel != null) errorPanel.SetActive(panel == errorPanel);
    }

    private void PopulateLanguageOptions()
    {
        // Clear existing options
        Transform container = isSelectingSourceLanguage ? sourceLanguageContainer : targetLanguageContainer;
        
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        
        // Add language options
        foreach (Language language in availableLanguages)
        {
            GameObject buttonObj = Instantiate(languageButtonPrefab, container);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = language.ToString();
            }
            
            if (button != null)
            {
                Language lang = language; // Make a copy for the closure
                button.onClick.AddListener(() => OnLanguageSelected(lang));
            }
        }
    }

    private void OnLanguageSelected(Language language)
    {
        if (isSelectingSourceLanguage)
        {
            TranslationManager.Instance.SetSourceLanguage(language);
        }
        else
        {
            TranslationManager.Instance.SetTargetLanguage(language);
        }
        
        // Update UI
        UpdateLanguageDisplay(
            TranslationManager.Instance.GetSourceLanguage(),
            TranslationManager.Instance.GetTargetLanguage()
        );
        
        // Return to main panel
        ShowMainPanel();
    }

    private void OnSwapLanguagesClicked()
    {
        TranslationManager.Instance.SwapLanguages();
    }
}
