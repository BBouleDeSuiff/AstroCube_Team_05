using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class GameActionShowSubtitle : AGameAction
{
    [SerializeField] private string _locutor;
    [SerializeField] private string _csvName;
    [SerializeField] private string _localizationID;
    [SerializeField] private float _duration;
    [SerializeField] private Color _color;
    
    public string CSVName => _csvName;
    public string LocalizationID => _localizationID;

    private bool _isFinished = true;
    
    protected override void ExecuteSpecific()
    {
        StartCoroutine(PrintSubtitle());
    }

    private IEnumerator PrintSubtitle()
    {
        _isFinished = false;
        //LocalizationManager.Instance.PrintStringFromID(_csvName, _localizationID, _locutor, _color);  // <-- Removed subtitles for the no-ui build
        if (AUDIO_ProgrammerInstrument.Instance != null)
            AUDIO_ProgrammerInstrument.Instance.PlayVoiceLine(_localizationID);
        yield return new WaitForSeconds(_duration);
        //LocalizationManager.Instance.ClearString();   // <-- Removed subtitles for the no-ui build
        _isFinished = true;
    }

    public override string BuildGameObjectName()
    {
        return $"SUBTITLE {_duration}s : <{_csvName}:{_localizationID}>";
    }
    
    protected override bool IsFinishedSpecific()
    {
        return _isFinished;
    }
    
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameActionShowSubtitle))]
public class ShowSubtitleCustomEditor : Editor
{
    private static string _currentText;
    private static Dictionary<(string csv, string id, ELanguage language), string> _texts;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GameActionShowSubtitle subtitle = (GameActionShowSubtitle)target;
        _texts = LocalizationManager.GenerateCSVInEditor();
        GUILayout.Space(20);
        
        if (_texts.TryGetValue((subtitle.CSVName, subtitle.LocalizationID, ELanguage.ENGLISH), out var text))
        {
            _currentText = text;
            GUI.color = Color.green;
            GUILayout.Label("Text Found", new GUIStyle(GUI.skin.label){fontStyle = FontStyle.Bold, fontSize = 15});
            GUI.color = Color.white;
            GUILayout.Label(_currentText);
        }
        else
        {
            GUI.color = Color.red;
            GUILayout.Label("Error", new GUIStyle(GUI.skin.label){fontStyle = FontStyle.Bold, fontSize = 15});
            GUI.color = Color.white;
            GUILayout.Label($"No text found for <{subtitle.CSVName}:{subtitle.LocalizationID}>");
        }
    }
}
#endif
