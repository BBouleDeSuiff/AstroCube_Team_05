using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using FMODUnity;

public class PauseMenuView : UIView
{
    private UIManager _uiManager;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private EventReference menuPauseSnapshot;
    [SerializeField] private EventReference openPauseMenu;
    [SerializeField] private EventReference closePauseMenu;

    private FMOD.Studio.EventInstance _menuPauseSnapshotInstance;
    private InputAction _cancelAction;
    private bool _isInitialized;
    private bool _keepSnapshotActive;
    private bool _ignoreCancelThisFrame;

    protected override void Awake()
    {
        base.Awake();
        _uiManager = FindObjectOfType<UIManager>();
    }

    private void OnEnable()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        restartButton.onClick.AddListener(OnRestartClicked);

        EventManager.OnGameUnpause += CloseMenu;

        var uiModule = EventSystem.current != null
            ? EventSystem.current.GetComponent<InputSystemUIInputModule>()
            : null;

        if (uiModule != null)
        {
            _cancelAction = uiModule.cancel;
            _cancelAction.performed += OnCancelPerformed;
        }

        if (!menuPauseSnapshot.IsNull && !_menuPauseSnapshotInstance.isValid())
        {
            _menuPauseSnapshotInstance = RuntimeManager.CreateInstance(menuPauseSnapshot);
            _menuPauseSnapshotInstance.start();
        }

        if (_isInitialized)
            RuntimeManager.PlayOneShot(openPauseMenu);

        _isInitialized = true;

        _ignoreCancelThisFrame = true;
        StartCoroutine(AllowCancelNextFrame());

        Time.timeScale = 0f;
        //Cursor.lockState = CursorLockMode.None;
    }

    private void OnDisable()
    {
        resumeButton.onClick.RemoveListener(OnResumeClicked);
        settingsButton.onClick.RemoveListener(OnSettingsClicked);
        quitButton.onClick.RemoveListener(OnQuitClicked);
        restartButton.onClick.RemoveListener(OnRestartClicked);

        EventManager.OnGameUnpause -= CloseMenu;

        if (_cancelAction != null)
            _cancelAction.performed -= OnCancelPerformed;

        _cancelAction = null;

        if (!_keepSnapshotActive && _menuPauseSnapshotInstance.isValid())
        {
            _menuPauseSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _menuPauseSnapshotInstance.release();
        }

        _keepSnapshotActive = false;
    }

    private IEnumerator AllowCancelNextFrame()
    {
        yield return null;
        _ignoreCancelThisFrame = false;
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (_ignoreCancelThisFrame)
            return;

        Time.timeScale = 1f;
        _uiManager.ShowInGameExclusive<PlayingView>();
    }

    private void OnResumeClicked()
    {
        Time.timeScale = 1f;
        //Cursor.lockState = CursorLockMode.Locked;
        EventManager.TriggerGameUnpause();
        
        InputSystemManager.Instance.PlayerInputs.DeactivateInput();
    }

    private void OnSettingsClicked()
    {
        _keepSnapshotActive = true;
        _uiManager.ShowInGameExclusive<SettingsMenuScreenView>();
    }

    private void OnQuitClicked()
    {
        var popup = _uiManager.ShowAndReturn<PopUpView>();
        if (popup == null)
            return;
        Hide();
        popup.ShowPopup(new PopUpData(
            title: "Quit Game ?",
            message: "",
            type: PopUpType.Warning,
            confirm: "Yes",
            cancel: "No",
            onConfirm: () =>
            {
                Time.timeScale = 1f;
                //Cursor.lockState = CursorLockMode.None;
                SceneManager.LoadScene("GameEntry");
            },
            onCancel: () =>
            {
                _uiManager.ShowInGameExclusive<PauseMenuView>();
            }
        ));
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void CloseMenu()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        _uiManager.ShowInGameExclusive<PlayingView>();
        RuntimeManager.PlayOneShot(closePauseMenu);
    }
}
