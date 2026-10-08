using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Localization")]
    [SerializeField] private ELanguage _currentLanguage = ELanguage.ENGLISH;
    [SerializeField] private TMP_Text _locutor;
    [SerializeField] private TextAutoSizing _textAutoSizing;
    
    [Header("Cutscene")]
    [SerializeField] RectTransform _upStrip;
    [SerializeField] RectTransform _downStrip;

    [Header("Skip")]
    [SerializeField] private CanvasGroup _skipGroup;
    [SerializeField] private List<Material> _skipMaterials = new();
    [SerializeField] private float _skipDisappearTime;

    private float _currentDisappearTime;
    private float _skipValue;
    private bool _isSkipCurrentlyActive = false;
    
    private Dictionary<(string csv, string id, ELanguage language), string> _idToDialog = new();
    private bool _stripsActive = false;

    private (string csv, string id)? _currentPrintedText;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;

        _currentLanguage = (ELanguage) PlayerPrefs.GetInt("LANGUAGE");
        
        GenerateCSV();
    }

    private void Update()
    {
        if (_isSkipCurrentlyActive && _skipValue == 0.0f)
        {
            _currentDisappearTime += Time.deltaTime;
            if (_currentDisappearTime >= _skipDisappearTime)
            {
                ShowSkip(false);
            }
        } else if (!_isSkipCurrentlyActive && _skipValue > 0.0f)
        {
            _currentDisappearTime = 0.0f;
            _isSkipCurrentlyActive = true;
            ShowSkip(true);
        } else if (_isSkipCurrentlyActive && _skipValue >= 1.0f)
        {
            _currentDisappearTime = _skipDisappearTime;
            _skipValue = 0.0f;
        }
    }

    public void GenerateCSV()
    {
        _idToDialog = new();
        var csvFiles = Resources.LoadAll<TextAsset>("Localization");
        foreach (TextAsset csv in csvFiles)
        {
            UnparseCSV(ref _idToDialog, csv);
        }

        foreach (Material m in _skipMaterials)
        {
            m.SetFloat("_Alpha", 0.0f);
        }
        _skipGroup.alpha = 0.0f;
    }

    public static Dictionary<(string csv, string id, ELanguage language), string> GenerateCSVInEditor()
    {
        Dictionary<(string csv, string id, ELanguage language), string> ed_idToDialog = new();
        var csvFiles = Resources.LoadAll<TextAsset>("Localization");
        foreach (TextAsset csv in csvFiles)
        {
            UnparseCSV(ref ed_idToDialog, csv);
        }
        
        return ed_idToDialog;
    }

    private static void UnparseCSV(ref Dictionary<(string csv, string id, ELanguage language), string> dict, TextAsset csv)
    {
        string csvName = csv.name;
        string[] lines = csv.text.Split('\n');
        string[] ids = lines[0].Split(';');

        for (var line = 1; line < lines.Length; line++)
        {
            string[] values = lines[line].Split(';');
            string id = values[0];
            for (var value = 1; value < values.Length - 1; value++)
            {
                if (Enum.TryParse(ids[value], out ELanguage language))
                {
                    dict[(csvName, id, language)] = values[value];
                }
                else
                {
                    throw new Exception("CSV columns are badly generated.");
                }
            }
        }
    }

    public string GetString(string csvName, string id)
    {
        if (_idToDialog.ContainsKey((csvName, id, _currentLanguage)))
        {
            string str = _idToDialog[(csvName, id, _currentLanguage)];
            return str;
        }
        else
        {
            Debug.LogWarning($"Didn't find any key corresponding to : {csvName}:{id}");
            return $"<{csvName}:{id}>";
        }
    }

    public string GetString(string fullId)
    {
        string csvName = fullId.Split(':')[0][1..];
        string id = fullId.Split(':')[1][..^1];
        Debug.Log($"<{csvName}:{id}>");
        
        if (_idToDialog.ContainsKey((csvName, id, _currentLanguage)))
        {
            string str = _idToDialog[(csvName, id, _currentLanguage)];
            return str;
        }
        else
        {
            Debug.LogWarning($"Didn't find any key corresponding to : {csvName}:{id}");
            return $"<{csvName}:{id}>";
        }
    }

    public void PrintString(string value, string locutor, Color? color = null)
    {
        _textAutoSizing.SetText(value, color);

        RectTransform rect = (RectTransform)_textAutoSizing.transform.GetChild(0);
        _locutor.rectTransform.position = new Vector3(
            rect.position.x, 
            rect.position.y + (rect.rect.height * rect.lossyScale.y / 2f) + 15.0f, 
            rect.position.z
        );
        _locutor.text = locutor;
        _locutor.color = color ?? Color.white;
        _locutor.gameObject.SetActive(true);
    }

    // Memory Object subtitles should be managed by the same system as normal subtitles. This should be made the case in Alula2.
    public void PrintStringFromID(string csvName, string id, string locutor, Color? color = null)
    {
        _currentPrintedText = (csvName, id);
        // PrintString(GetString(csvName, id), locutor, color);     // <-- removing Memory Object Subtitles for no-ui build
    }

    public void ClearString()
    {
        _currentPrintedText = null;
        _textAutoSizing.SetText("", null);
        _locutor.gameObject.SetActive(false);
    }
    
    public void SetStrips(bool state, float animationDuration)
    {
        _stripsActive = state;
        if (!state)
        {
            _upStrip.DOAnchorPosY(110f, animationDuration);
            _downStrip.DOAnchorPosY(-110f, animationDuration);
        }
        else
        {
            _upStrip.DOAnchorPosY(-65.745f, animationDuration);
            _downStrip.DOAnchorPosY(65.745f, animationDuration);
        }
    }

    public void SetSkipValue(float value)
    {
        _skipValue = Mathf.Clamp01(value);
        
        foreach(Material m in _skipMaterials)
            m.SetFloat("_Slider1", _skipValue);
    }
    
    private void ShowSkip(bool state)
    {
        _isSkipCurrentlyActive = state;
        foreach (Material m in _skipMaterials)
        {
            DOTween.To(() => m.GetFloat("_Alpha"), (x) => m.SetFloat("_Alpha", x), state ? 1.0f : 0.0f, 1.0f);
        }
        _skipGroup.DOFade(state ? 1.0f : 0.0f, 1.0f);
    }

    public void SwitchLanguage(int value)
    {
        int language = (int) Mathf.Repeat((float) _currentLanguage + value, 2);
        _currentLanguage = (ELanguage) language;
        
        PlayerPrefs.SetInt("LANGUAGE", (int) _currentLanguage);

        UpdateTexts();
    }

    private void UpdateTexts()
    {
        foreach (UIToggleButton button in FindObjectsByType<UIToggleButton>(FindObjectsInactive.Include,
                     FindObjectsSortMode.InstanceID))
        {
            button.RefreshUI();
        }
        
        FindFirstObjectByType<SettingsMenuScreenView>(FindObjectsInactive.Include).UpdateHoverText();
        
        foreach(LocalizedText txt in FindObjectsByType<LocalizedText>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
        {
            txt.UpdateText();
        }

        if (_currentPrintedText != null)
        {
            PrintStringFromID(_currentPrintedText.Value.csv, _currentPrintedText.Value.id, _locutor.text, _locutor.color);
        }
    }

    #if UNITY_EDITOR
    [MenuItem("Tools/Update TMP Texts")]
    private static void UpdateTMPTexts()
    {
        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            if (text.GetComponent<LocalizedText>() == null)
            {
                text.gameObject.AddComponent<LocalizedText>();
            }
        }
    }
    #endif
}

public enum ELanguage
{
    ENGLISH = 0,
    FRENCH = 1,
    LANGUAGE_3 = 2,
    LANGUAGE_4 = 3,
    LANGUAGE_5 = 4,
    LANGUAGE_6 = 5,
    LANGUAGE_7 = 6,
    LANGUAGE_8 = 7,
    LANGUAGE_9 = 8,
    LANGUAGE_10 = 9
}
