using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class SettingsMenuScreenView : UIView
{
    [Header("(REQUIRED)")]
    [SerializeField] private bool isInGameplay = false;

    [Header("Description Part")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Sound Settings")]
    [SerializeField] private Slider generalSoundSlider;
    [SerializeField] private Slider musicSoundSlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider voiceSoundSlider;

    [Header("Camera Settings")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider cameraSensitivitySlider;
    [SerializeField] private UIToggleButton motionBlurButton;

    [Header("Accessibility Settings")]
    [SerializeField] private Button languageButton;
    [SerializeField] private UIToggleButton rumbleButton;
    [SerializeField] private UIToggleButton previewButton;
    [SerializeField] private UIToggleButton oneHandedButton;
    [SerializeField] private Button _previousLanguageButton, _nextLanguageButton;

    [Header("Others")]
    [SerializeField] private Button backButton;

    [Header("Settings References")]
    [SerializeField] private CustomisedSettings _customisedSettings;

    [Header("FMOD References")]
    [SerializeField] private FMODUnity.EventReference menuPauseSnapshot;
    private FMOD.Studio.EventInstance _menuPauseSnapshotInstance;


    private bool _isInitializing = false;
    private UIManager _uiManager;
    private Dictionary<string, string> _descriptionBySettings;

    private InputAction _cancelAction;

    private string _currentKey = "";


    private void Awake()
    {
        base.Awake();
        _uiManager = FindObjectOfType<UIManager>();

        _descriptionBySettings = new Dictionary<string, string>()
        {
            { "<MENU:GENERAL>", "<MENU:GENERAL_DESC>" },
            { "<MENU:MUSIC>", "<MENU:MUSIC_DESC>"},
            { "<MENU:SOUND>", "<MENU:SOUND_DESC>"},
            { "<MENU:VOICE>", "<MENU:VOICE_DESC>"},

            { "<MENU:FOV>", "<MENU:FOV_DESC>"},
            { "<MENU:CAMERA_SENSITIVITY>", "<MENU:SENSITIVITY_DESC>"},
            { "<MENU:MOTION_BLUR>", "<MENU:MOTION_BLUR_DESC>"},

            { "<MENU:LANGUAGE>", "<MENU:LANGUAGE_DESC>"},
            { "<MENU:RUMBLE>", "<MENU:RUMBLE_DESC>"},
            { "<MENU:PREVIEW>", "<MENU:PREVIEW_DESC>"},
            { "<MENU:ONE_HAND_MODE>", "<MENU:ONE_HAND_MODE_DESC>"}
        };

        _customisedSettings.LoadRuntimeValues();
        SetupUI();
        SetupHover();
    }

    private void SetupUI()
    {
        _isInitializing = true;

        if (PlayerPrefs.HasKey("MasterVolume"))
            _customisedSettings.customVolume = PlayerPrefs.GetFloat("MasterVolume");
        if (PlayerPrefs.HasKey("MusicVolume"))
            _customisedSettings.customMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        if (PlayerPrefs.HasKey("SFXVolume"))
            _customisedSettings.customSoundEffectsVolume = PlayerPrefs.GetFloat("SFXVolume");
        if (PlayerPrefs.HasKey("VoiceVolume"))
            _customisedSettings.customVoiceVolume = PlayerPrefs.GetFloat("VoiceVolume");

        generalSoundSlider.onValueChanged.AddListener(OnGeneralSoundSliderValueChanged);
        musicSoundSlider.onValueChanged.AddListener(OnMusicSoundSliderValueChanged);
        soundSlider.onValueChanged.AddListener(OnSoundSoundSliderValueChanged);
        voiceSoundSlider.onValueChanged.AddListener(OnVoiceSoundSliderValueChanged);

        fovSlider.onValueChanged.AddListener(OnFovSliderChanged);
        cameraSensitivitySlider.onValueChanged.AddListener(OnCameraSensitivityChanged);

        motionBlurButton.onToggleChanged.AddListener(OnMotionBlurToggled);
        rumbleButton.onToggleChanged.AddListener(OnRumbleToggled);
        previewButton.onToggleChanged.AddListener(OnPreviewToggled);
        oneHandedButton.onToggleChanged.AddListener(OnOneHandToggled);
        
        _nextLanguageButton.onClick.AddListener(() => LocalizationManager.Instance.SwitchLanguage(1));
        _previousLanguageButton.onClick.AddListener(() => LocalizationManager.Instance.SwitchLanguage(-1));

        backButton.onClick.AddListener(OnQuitClicked);

        motionBlurButton.SetState(_customisedSettings.customMotionBlur, false);
        rumbleButton.SetState(_customisedSettings.customVibration, false);
        previewButton.SetState(_customisedSettings.customPreview, false);
        oneHandedButton.SetState(_customisedSettings.customOneHandMode, false);

        fovSlider.minValue = _customisedSettings.minFOV;
        fovSlider.maxValue = _customisedSettings.maxFOV;
        fovSlider.value = _customisedSettings.customFov;

        cameraSensitivitySlider.minValue = _customisedSettings.minMouse;
        cameraSensitivitySlider.maxValue = _customisedSettings.maxMouse;
        cameraSensitivitySlider.value = _customisedSettings.customMouse;

        generalSoundSlider.minValue = _customisedSettings.minVolume;
        generalSoundSlider.maxValue = _customisedSettings.maxVolume;
        generalSoundSlider.value = _customisedSettings.customVolume;

        musicSoundSlider.minValue = _customisedSettings.minMusicVolume;
        musicSoundSlider.maxValue = _customisedSettings.maxMusicVolume;
        musicSoundSlider.value = _customisedSettings.customMusicVolume;

        soundSlider.minValue = _customisedSettings.minSoundEffectsVolume;
        soundSlider.maxValue = _customisedSettings.maxSoundEffectsVolume;
        soundSlider.value = _customisedSettings.customSoundEffectsVolume;

        voiceSoundSlider.minValue = _customisedSettings.minVoiceVolume;
        voiceSoundSlider.maxValue = _customisedSettings.maxVoiceVolume;
        voiceSoundSlider.value = _customisedSettings.customVoiceVolume;

        ApplySavedSoundValues();

        _isInitializing = false;

        Debug.Log($"[Init Settings] MotionBlur={_customisedSettings.customMotionBlur}, Rumble={_customisedSettings.customVibration}, Preview={_customisedSettings.customPreview}");
    }

    private void ApplySavedSoundValues()
    {
        FMOD.Studio.VCA masterVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Master");
        FMOD.Studio.VCA musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Music");
        FMOD.Studio.VCA sfxVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Effect");
        FMOD.Studio.VCA voiceVCA = FMODUnity.RuntimeManager.GetVCA("vca:/VO");

        masterVCA.setVolume(_customisedSettings.customVolume);
        musicVCA.setVolume(_customisedSettings.customMusicVolume);
        sfxVCA.setVolume(_customisedSettings.customSoundEffectsVolume);
        voiceVCA.setVolume(_customisedSettings.customVoiceVolume);

        generalSoundSlider.SetValueWithoutNotify(_customisedSettings.customVolume);
        musicSoundSlider.SetValueWithoutNotify(_customisedSettings.customMusicVolume);
        soundSlider.SetValueWithoutNotify(_customisedSettings.customSoundEffectsVolume);
        voiceSoundSlider.SetValueWithoutNotify(_customisedSettings.customVoiceVolume);

        Debug.Log("[FMOD Sync] Restored saved VCA volumes");
    }

    #region Button Methods

    public override void Show()
    {
        base.Show();
        titleText.text = "";
        descriptionText.text = "";
    }

    private void OnQuitClicked()
    {
        Hide();
        if (isInGameplay)
            _uiManager.ShowInGameExclusive<PauseMenuView>();
        else
            _uiManager.Show<MainMenuView>();
    }

    private void OnMotionBlurToggled(bool state)
    {
        _customisedSettings.customMotionBlur = state;
        _customisedSettings.SaveRuntimeValues();
    }

    private void OnRumbleToggled(bool state)
    {
        _customisedSettings.customVibration = state;
        _customisedSettings.SaveRuntimeValues();
    }

    private void OnPreviewToggled(bool state)
    {
        _customisedSettings.customPreview = state;
        _customisedSettings.SaveRuntimeValues();
    }

    private void OnOneHandToggled(bool state)
    {
        _customisedSettings.customOneHandMode = state;
        _customisedSettings.SaveRuntimeValues();
    }

    public void OnGeneralSoundSliderValueChanged(float value)
    {
        if (_isInitializing) return;

        FMOD.Studio.VCA masterVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Master");
        masterVCA.setVolume(value);

        _customisedSettings.customVolume = value;
        _customisedSettings.SaveRuntimeValues();

        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    private void OnMusicSoundSliderValueChanged(float value)
    {
        if (_isInitializing) return;

        FMOD.Studio.VCA musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Music");
        musicVCA.setVolume(value);

        _customisedSettings.customMusicVolume = value;
        _customisedSettings.SaveRuntimeValues();

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    private void OnSoundSoundSliderValueChanged(float value)
    {
        if (_isInitializing) return;

        FMOD.Studio.VCA sfxVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Effect");
        sfxVCA.setVolume(value);

        _customisedSettings.customSoundEffectsVolume = value;
        _customisedSettings.SaveRuntimeValues();

        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    private void OnVoiceSoundSliderValueChanged(float value)
    {
        if (_isInitializing) return;

        FMOD.Studio.VCA voiceVCA = FMODUnity.RuntimeManager.GetVCA("vca:/VO");
        voiceVCA.setVolume(value);

        _customisedSettings.customVoiceVolume = value;
        _customisedSettings.SaveRuntimeValues();

        PlayerPrefs.SetFloat("VoiceVolume", value);
        PlayerPrefs.Save();
    }

    public void OnFovSliderChanged(float value)
    {
        if (_isInitializing) return;

        _customisedSettings.customFov = value;
        _customisedSettings.SaveRuntimeValues();
    }

    public void OnCameraSensitivityChanged(float value)
    {
        if (_isInitializing) return;

        _customisedSettings.customMouse = value;
        _customisedSettings.SaveRuntimeValues();
    }

    #endregion

    private void OnEnable()
    {
        base.OnEnable(); 

        var uiModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        _cancelAction = uiModule.cancel;

        // Ne pas créer le snapshot si on vient du jeu (pause menu), car il est déjà actif
        if (!isInGameplay && !menuPauseSnapshot.IsNull)
        {
            _menuPauseSnapshotInstance = RuntimeManager.CreateInstance(menuPauseSnapshot);
            _menuPauseSnapshotInstance.start();
        }

        if (isInGameplay)
        {
            EventManager.OnGameUnpause += CloseMenu;
            _cancelAction.performed += OnCancelPerformed;
        }
        else
        {
            _cancelAction.performed += OnCancelPerformed;
        }
    }


    private void OnDisable()
    {
        base.OnDisable(); 

        // Ne pas stopper le snapshot si on est en gameplay (on retourne au pause menu)
        if (!isInGameplay && _menuPauseSnapshotInstance.isValid())
        {
            _menuPauseSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _menuPauseSnapshotInstance.release();
        }

        if (isInGameplay)
        {
            EventManager.OnGameUnpause -= CloseMenu;
            _cancelAction.performed -= OnCancelPerformed;
        }
        else
        {
            _cancelAction.performed -= OnCancelPerformed;
        }
    }


    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        _uiManager.ShowInGameExclusive<PauseMenuView>();
    }

    private void CloseMenu()
    {

        //Cursor.lockState = CursorLockMode.Locked;
        _uiManager.ShowInGameExclusive<PlayingView>();
    }

    private void BackToMainMenu()
    {
        Debug.Log("Back to Main Menu from Settings Menu");
        Hide();
        //Cursor.lockState = CursorLockMode.None;
        _uiManager.Show<MainMenuView>();
    }

    #region Hover Implementation

    private void OnSettingSelected(string key)
    {
        if (_descriptionBySettings.TryGetValue(key, out var description))
        {
            titleText.text = LocalizationManager.Instance.GetString(key);
            descriptionText.text = LocalizationManager.Instance.GetString(description);
        }
    }

    private void SetupHover()
    {
        AddHover(generalSoundSlider.gameObject, "<MENU:GENERAL>");
        AddHover(musicSoundSlider.gameObject, "<MENU:MUSIC>");
        AddHover(soundSlider.gameObject, "<MENU:SOUND>");
        AddHover(voiceSoundSlider.gameObject, "<MENU:VOICE>");

        AddHover(fovSlider.gameObject, "<MENU:FOV>");
        AddHover(cameraSensitivitySlider.gameObject, "<MENU:CAMERA_SENSITIVITY>");
        AddHover(motionBlurButton.gameObject, "<MENU:MOTION_BLUR>");

        AddHover(languageButton.gameObject, "<MENU:LANGUAGE>");
        AddHover(rumbleButton.gameObject, "<MENU:RUMBLE>");
        AddHover(previewButton.gameObject, "<MENU:PREVIEW>");
        AddHover(oneHandedButton.gameObject, "<MENU:ONE_HAND_MODE>");
    }

    public void OnSettingHovered(string key)
    {
        if (_descriptionBySettings.TryGetValue(key, out var description))
        {
            titleText.text = LocalizationManager.Instance.GetString(key);
            descriptionText.text = LocalizationManager.Instance.GetString(description);
        }
    }

    public void UpdateHoverText()
    {
        if (_currentKey != "")
        {
            if (_descriptionBySettings.TryGetValue(_currentKey, out var description))
            {
                titleText.text = LocalizationManager.Instance.GetString(_currentKey);
                descriptionText.text = LocalizationManager.Instance.GetString(description);
            }
        }
    }

    public void SetCurrentKey(string key) => _currentKey = key;

    private void AddHover(GameObject obj, string key)
    {
        var hover = obj.GetComponent<SettingsHoverElement>();
        if (hover == null)
            hover = obj.AddComponent<SettingsHoverElement>();
        hover.Initialize(this, key);
    }

    #endregion
}
