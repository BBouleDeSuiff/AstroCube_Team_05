using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "CustomisedSettings", menuName = "ScriptableObjects/CustomisedSettings", order = 2)]
public class CustomisedSettings : ScriptableObject
{


    public float defaultFOV => _defaultFOV;
    public float minFOV => _minimumFOV;
    public float maxFOV => _maximumFOV;
    public float customFov
    {
        get => _customFov ?? defaultFOV;
        set
        {
            if (value < minFOV)
                _customFov = minFOV;
            else if (value > maxFOV)
                _customFov = maxFOV;
            else
                _customFov = value;

            EventManager.TriggerFOVChange(_customFov.HasValue ? _customFov.Value : defaultFOV);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public float defaultMouse => _defaultMouse;
    public float minMouse => _minimumMouse;
    public float maxMouse => _maximumMouse;
    public float customMouse
    {
        get => _customMouse ?? defaultMouse;
        set
        {
            if (value < minMouse)
                _customMouse = minMouse;
            else if (value > maxMouse)
                _customMouse = maxMouse;
            else
                _customMouse = value;

            EventManager.TriggerMouseChange(_customMouse.Value);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public float defaultVolume => _defaultVolume;
    public float minVolume => _minimumVolume;
    public float maxVolume => _maximumVolume;
    public float customVolume
    {
        get => _customGeneralVolume ?? defaultVolume;
        set
        {
            if (value < minVolume)
                _customGeneralVolume = minVolume;
            else if (value > maxVolume)
                _customGeneralVolume = maxVolume;
            else
                _customGeneralVolume = value;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public float defaultMusicVolume => _defaultMusicVolume;
    public float minMusicVolume => _minimumMusicVolume;
    public float maxMusicVolume => _maximumMusicVolume;
    public float customMusicVolume
    {
        get => _customMusicVolume ?? defaultMusicVolume;
        set
        {
            if (value < minMusicVolume)
                _customMusicVolume = minMusicVolume;
            else if (value > maxMusicVolume)
                _customMusicVolume = maxMusicVolume;
            else
                _customMusicVolume = value;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public float defaultSoundEffectsVolume => _defaultSoundEffectsVolume;
    public float minSoundEffectsVolume => _minimumSoundEffectsVolume;
    public float maxSoundEffectsVolume => _maximumSoundEffectsVolume;
    public float customSoundEffectsVolume
    {
        get => _customSoundEffectsVolume ?? defaultSoundEffectsVolume;
        set
        {
            if (value < minSoundEffectsVolume)
                _customSoundEffectsVolume = minSoundEffectsVolume;
            else if (value > maxSoundEffectsVolume)
                _customSoundEffectsVolume = maxSoundEffectsVolume;
            else
                _customSoundEffectsVolume = value;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    public float defaultVoiceVolume => _defaultVoiceVolume;
    public float minVoiceVolume => _minimumVoiceVolume;
    public float maxVoiceVolume => _maximumVoiceVolume;
    public float customVoiceVolume
    {
        get => _customVoiceVolume ?? defaultVoiceVolume;
        set
        {
            if (value < minVoiceVolume)
                _customVoiceVolume = minVoiceVolume;
            else if (value > maxVoiceVolume)
                _customVoiceVolume = maxVoiceVolume;
            else
                _customVoiceVolume = value;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }



    public bool defaultVibration => _defaultVibration;
    public bool customVibration
    {
        get => _customVibration ?? _defaultVibration;
        set
        {
            _customVibration = value;
            EventManager.TriggerVibrationChange(customVibration);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public bool defaultMotionBlur => _defaultMotionBlur;
    public bool customMotionBlur
    {
        get => _customMotionBlur ?? _defaultMotionBlur;
        set
        {
            _customMotionBlur = value;
            EventManager.TriggerMotionBlurChange(customMotionBlur);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public bool defaultOneHandMode => _defaultOneHandMode;
    public bool customOneHandMode
    {
        get => _customOneHandMode ?? defaultOneHandMode;
        set
        {
            _customOneHandMode = value;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public bool defaultCorrectActions => _defaultCorrectActions;
    public bool customCorrectActions
    {
        get => _customCorrectActions ?? defaultCorrectActions;
        set
        {
            _customCorrectActions = value;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public bool defaultPreview => _defaultPreview;
    public bool customPreview
    {
        get => _customPreview ?? defaultPreview;
        set
        {
            _customPreview = value;
            EventManager.TriggerPreviewChange(customPreview);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public bool defaultSubtitles => _defaultSubtitles;
    public bool customSubtitles
    {
        get => _customSubtitles ?? defaultSubtitles;
        set
        {
            _customSubtitles = value;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    [Header("FOV")]
    [SerializeField, Label("Default")] float _defaultFOV;
    [SerializeField, Label("Minimum")] float _minimumFOV;
    [SerializeField, Label("Maximum")] float _maximumFOV;
    float? _customFov;

    [Header("Mouse Sensibility")]
    [SerializeField, Label("Default")] float _defaultMouse;
    [SerializeField, Label("Minimum")] float _minimumMouse;
    [SerializeField, Label("Maximum")] float _maximumMouse;
    float? _customMouse;

    [Header("Volume")]
    [SerializeField, Label("Default")] float _defaultVolume;
    [SerializeField, Label("Minimum")] float _minimumVolume;
    [SerializeField, Label("Maximum")] float _maximumVolume;
    float? _customGeneralVolume;

    [SerializeField, Label("Default")] float _defaultMusicVolume;
    [SerializeField, Label("Minimum")] float _minimumMusicVolume;
    [SerializeField, Label("Maximum")] float _maximumMusicVolume;
    float? _customMusicVolume;

    [SerializeField, Label("Default")] float _defaultSoundEffectsVolume;
    [SerializeField, Label("Minimum")] float _minimumSoundEffectsVolume;
    [SerializeField, Label("Maximum")] float _maximumSoundEffectsVolume;
    float? _customSoundEffectsVolume;

    [SerializeField, Label("Default")] float _defaultVoiceVolume;
    [SerializeField, Label("Minimum")] float _minimumVoiceVolume;
    [SerializeField, Label("Maximum")] float _maximumVoiceVolume;
    float? _customVoiceVolume;



    [Header("Vibration")]
    [SerializeField, Label("Default")] bool _defaultVibration;
    bool? _customVibration;

    [Header("Motion Blur")]
    [SerializeField, Label("Default")] bool _defaultMotionBlur;
    bool? _customMotionBlur;

    [Header("Preview")]
    [SerializeField, Label("Default")] bool _defaultPreview;
    bool? _customPreview;

    [Header("Subtitles")]
    [SerializeField, Label("Default")] bool _defaultSubtitles;
    bool? _customSubtitles;

    [Header("One Hand Mode")]
    [SerializeField, Label("Default")] bool _defaultOneHandMode;
    bool? _customOneHandMode;

    [Header("Correct Actions")]
    [SerializeField, Label("Default")] bool _defaultCorrectActions;
    bool? _customCorrectActions;


    public void SaveRuntimeValues()
    {
        PlayerPrefs.SetFloat("Setting_Volume_General", customVolume);
        PlayerPrefs.SetFloat("Setting_Volume_Music", customMusicVolume);
        PlayerPrefs.SetFloat("Setting_Volume_SFX", customSoundEffectsVolume);
        PlayerPrefs.SetFloat("Setting_Volume_Voice", customVoiceVolume);

        PlayerPrefs.SetInt("Setting_MotionBlur", customMotionBlur ? 1 : 0);
        PlayerPrefs.SetInt("Setting_Vibration", customVibration ? 1 : 0);
        PlayerPrefs.SetInt("Setting_Preview", customPreview ? 1 : 0);
        PlayerPrefs.SetInt("Setting_Subtitles", customSubtitles ? 1 : 0);
        PlayerPrefs.SetInt("Setting_OneHandMode", customOneHandMode ? 1 : 0);
        PlayerPrefs.SetInt("Setting_CorrectActions", customCorrectActions ? 1 : 0);

        PlayerPrefs.SetFloat("Setting_FOV", customFov);
        PlayerPrefs.SetFloat("Setting_MouseSensitivity", customMouse);

        PlayerPrefs.Save();
    }

    public void LoadRuntimeValues()
    {
        if (PlayerPrefs.HasKey("Setting_Volume_General"))
        {
            float general = PlayerPrefs.GetFloat("Setting_Volume_General");
            //Debug.Log($"[LoadRuntimeValues] Loaded GeneralVolume from prefs: {general}");
            _customGeneralVolume = general;
        }
        else
        {
            _customGeneralVolume = defaultVolume;
        }

        if (PlayerPrefs.HasKey("Setting_Volume_Music"))
        {
            float music = PlayerPrefs.GetFloat("Setting_Volume_Music");
            //Debug.Log($"[LoadRuntimeValues] Loaded MusicVolume from prefs: {music}");
            _customMusicVolume = music;
        }
        else
        {
            _customMusicVolume = defaultMusicVolume;
        }

        if (PlayerPrefs.HasKey("Setting_Volume_SFX"))
        {
            float sfx = PlayerPrefs.GetFloat("Setting_Volume_SFX");
            //Debug.Log($"[LoadRuntimeValues] Loaded SFXVolume from prefs: {sfx}");
            _customSoundEffectsVolume = sfx;
        }
        else
        {
            _customSoundEffectsVolume = defaultSoundEffectsVolume;
        }

        if (PlayerPrefs.HasKey("Setting_Volume_Voice"))
        {
            float voice = PlayerPrefs.GetFloat("Setting_Volume_Voice");
            //Debug.Log($"[LoadRuntimeValues] Loaded VoiceVolume from prefs: {voice}");
            _customVoiceVolume = voice;
        }
        else
        {
            _customVoiceVolume = defaultVoiceVolume;
        }

        if (PlayerPrefs.HasKey("Setting_FOV"))
        {
            float fov = PlayerPrefs.GetFloat("Setting_FOV");
            //Debug.Log($"[LoadRuntimeValues] Loaded FOV from prefs: {fov}");
            _customFov = fov;
        }
        else
        {
            _customFov = defaultFOV;
        }

      
        if(PlayerPrefs.HasKey("OneHandMode"))
        {
            customOneHandMode = PlayerPrefs.GetInt("OneHandMode") == 1;
        }
        else
        {
            customOneHandMode = defaultOneHandMode;
        }

        if (PlayerPrefs.HasKey("CorrectActions"))
        {
            customCorrectActions = PlayerPrefs.GetInt("CorrectActions") == 1;
        }
        else
        {
            customCorrectActions = defaultCorrectActions;
        }

        if (PlayerPrefs.HasKey("Setting_MouseSensitivity"))
        {
            float sens = PlayerPrefs.GetFloat("Setting_MouseSensitivity");
            //Debug.Log($"[LoadRuntimeValues] Loaded MouseSensitivity from prefs: {sens}");
            _customMouse = sens;
        }

        if (PlayerPrefs.HasKey("Setting_MotionBlur"))
            customMotionBlur = PlayerPrefs.GetInt("Setting_MotionBlur") == 1;
        if (PlayerPrefs.HasKey("Setting_Vibration"))
            customVibration = PlayerPrefs.GetInt("Setting_Vibration") == 1;
        if (PlayerPrefs.HasKey("Setting_Preview"))
            customPreview = PlayerPrefs.GetInt("Setting_Preview") == 1;
    }
}
