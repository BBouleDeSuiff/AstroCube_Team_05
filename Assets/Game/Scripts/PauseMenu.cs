using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMODUnity;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject _UIHolder;
    [SerializeField] GameObject _firstSelected;
    [SerializeField] TextMeshProUGUI _sceneName;
    [SerializeReference] GameObject SettingsUIHolder;
    [SerializeField] GameObject ControlsUIHolder;
    [SerializeField] EventReference menuPauseSnapshot;
    
    PostProcessManager _kaleidoscopeManager;
    private FMOD.Studio.EventInstance _menuPauseSnapshotInstance;

    private void Start()
    {
        _kaleidoscopeManager = GetComponentInChildren<PostProcessManager>();
    }

    private void OnEnable()
    {
        //EventManager.OnGamePause += OpenMenu;
        //EventManager.OnGameUnpause += CloseMenu;
    }

    private void OnDisable()
    {
        //EventManager.OnGamePause -= OpenMenu;
        //EventManager.OnGameUnpause -= CloseMenu;
    }

    void OpenMenu()
    {
        if(_kaleidoscopeManager) _kaleidoscopeManager.SetEnabled(true);
        _UIHolder.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_firstSelected);
        _sceneName.text = SceneManager.GetActiveScene().name;

        _menuPauseSnapshotInstance = RuntimeManager.CreateInstance(menuPauseSnapshot);
        _menuPauseSnapshotInstance.start();
    }

    void CloseMenu()
    {
        if (_kaleidoscopeManager) _kaleidoscopeManager.SetEnabled(false);
        _UIHolder.SetActive(false);
        SettingsUIHolder.SetActive(false);
        ControlsUIHolder.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;

        if (_menuPauseSnapshotInstance.isValid())
        {
            _menuPauseSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _menuPauseSnapshotInstance.release();
        }
    }

    public void SetActiveSettingsMenu(bool isActive = true)
    {
        _UIHolder.SetActive(!isActive);
        SettingsUIHolder.SetActive(isActive);
    }
}
