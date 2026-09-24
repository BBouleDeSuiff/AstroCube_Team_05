using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using RubiksStatic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public GameSettings Settings => settings;

    [SerializeField] private GameSettings settings;

    public CustomisedSettings CustomSettings => CutomizeSettings;
    [SerializeField] private CustomisedSettings CutomizeSettings;
        
    //[SerializeField][Scene] string nextScene;

    [Header("Entity Sequence")]
    [SerializeField] EntitySequenceManager _entitySequenceManager;
    [SerializeField] Image _fade;
    [SerializeField] float _sequenceDuration;
    [SerializeField] Transform _artifact;
    [SerializeField] List<GameObject> _objectToDisable;
    [SerializeField] ExitDoor _endPortal;
    CameraAnimator _cameraAnimator;
    PlayerMovement _player;

    public static GameManager Instance => instance;
    private static GameManager instance;

    [SerializeField] private RubiksMovement _rubiksCube;

    [SerializeField] private GameObject _rubiksCubeUI;
    public bool IsUIRubiksCubeEnabled => isUIRubiksCubeEnabled;
    [FormerlySerializedAs("_isRubiksCubeEnabled")] [SerializeField, ReadOnly] private bool isUIRubiksCubeEnabled;

    [field: SerializeField] public PreviewRubiksCube PreviewRubiksCube { get; private set; }

    public SliceAxis ActualSliceAxis { get => _actualSliceAxis; set => _actualSliceAxis = value; }




    [SerializeField] private SliceAxis _actualSliceAxis;
    public PositionSaveFile RightActions { get; private set; }
    public RubiksMovement RubiksCube { get => _rubiksCube; set => _rubiksCube = value; }
    public PlayerMovement Player { get => _player; set => _player = value; }

    private void Awake()
    {
        if (instance) Destroy(this);
        else instance = this;

        RightActions = Resources.Load<PositionSaveFile>($"CorrectActionsSaves/CorrectActionsSave_{SceneManager.GetActiveScene().name}");

        if (PreviewRubiksCube == null)
        {
            PreviewRubiksCube = FindAnyObjectByType<PreviewRubiksCube>();
        }
        Player = FindAnyObjectByType<PlayerMovement>();
    }

    public enum EScreenshakeMode
    {
        START_RUBIKS_CUBE_ROTATION,
        END_RUBIKS_CUBE_ROTATION
    }

    public void Screenshake(EScreenshakeMode mode)
    {
        switch (mode)
        {
            default:
                break;
            case EScreenshakeMode.END_RUBIKS_CUBE_ROTATION:
                Camera.main.DOShakePosition(settings.RubiksEndCubeRotationScreenshakeSettings.x,
                                            settings.RubiksEndCubeRotationScreenshakeSettings.y,
                                            (int)settings.RubiksEndCubeRotationScreenshakeSettings.z,
                                            settings.RubiksEndCubeRotationScreenshakeSettings.w,
                                            false,
                                            ShakeRandomnessMode.Full);
                break;
            case EScreenshakeMode.START_RUBIKS_CUBE_ROTATION:
                Camera.main.DOShakePosition(settings.RubikscCubeAxisRotationDuration,
                                            settings.RubiksStartCubeRotationScreenshakeSettings.y,
                                            (int)settings.RubiksStartCubeRotationScreenshakeSettings.z,
                                            settings.RubiksStartCubeRotationScreenshakeSettings.w,
                                            false,
                                            ShakeRandomnessMode.Full);
                break;
        }
    }

    void ScreenshakeCubeRotationStart() => Screenshake(EScreenshakeMode.START_RUBIKS_CUBE_ROTATION);
    void ScreenshakeCubeRotationEnd() => Screenshake(EScreenshakeMode.END_RUBIKS_CUBE_ROTATION);


    private void OnEnable()
    {
        EventManager.OnSceneChange += ChangeScene;

        EventManager.OnStartCubeRotation += ScreenshakeCubeRotationStart;
        EventManager.OnEndCubeRotation += ScreenshakeCubeRotationEnd;

        EventManager.OnGamePause += StopDeltaTime;
        EventManager.OnGameUnpause += ResetDeltaTime;

        EventManager.OnGamePause += UnlockMouse;
        EventManager.OnGameUnpause += LockMouse;
    }

    private void OnDisable()
    {
        EventManager.OnSceneChange -= ChangeScene;

        EventManager.OnStartCubeRotation -= ScreenshakeCubeRotationStart;
        EventManager.OnEndCubeRotation -= ScreenshakeCubeRotationEnd;

        EventManager.OnGamePause -= StopDeltaTime;
        EventManager.OnGameUnpause -= ResetDeltaTime;
        EventManager.OnGameUnpause -= LockMouse;
        EventManager.OnGamePause -= UnlockMouse;
    }

    private void Start()
    {
        EventManager.TriggerSceneStart();
        LockMouse();
    }
    public void ChangeScene()
    {
        SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
        LockMouse();
    }

    public void LoadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void StopDeltaTime()
    {
        Time.timeScale = 0.0f;
    }    
    
    void ResetDeltaTime()
    {
        Time.timeScale = 1f;
    }

    void LockMouse()
    {
        if (InputSystemManager.Instance.CurrentInputMode == InputSystemManager.EInputMode.KEYBOARD)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void UnlockMouse()
    {
        if (InputSystemManager.Instance.CurrentInputMode == InputSystemManager.EInputMode.KEYBOARD || Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void StartSequence(Quaternion cameraAngle) => StartCoroutine(ShowSequence(cameraAngle));
    public IEnumerator ShowSequence(Quaternion cameraAngle)
    {
        EventManager.TriggerNarrativeSequenceStart();

        yield return DOTween.To(() => new Color(0, 0, 0, 0), x => _fade.color = x, new Color(0, 0, 0, 1.0f), 1.0f).WaitForCompletion();
        _fade.color = new Color(0, 0, 0, 0);

        foreach (var obj in _objectToDisable)
        {
            if(obj)
                obj.gameObject.SetActive(false);
        }
        _artifact.gameObject.SetActive(false);
        _entitySequenceManager.gameObject.SetActive(true);

        EnableRubiksCube();
        
        yield return new WaitForSeconds(_sequenceDuration);

        _entitySequenceManager.gameObject.SetActive(false);

        foreach (var obj in _objectToDisable)
        {
            if (obj)
                obj.gameObject.SetActive(true);
            if (obj == null) continue;
            obj.gameObject.SetActive(true);
        }

        EventManager.TriggerNarrativeSequenceEnd();
        _fade.color = new Color(0, 0, 0, 1.0f);

        _cameraAnimator = Camera.main.transform.parent.parent.GetComponent<CameraAnimator>(); 
        _cameraAnimator.transform.rotation = Quaternion.Euler(0, cameraAngle.eulerAngles.y - 180, 0);
        _cameraAnimator.transform.position = new Vector3(0, _cameraAnimator.transform.position.y, 0);
        Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);

        yield return DOTween.To(() => new Color(0, 0, 0, 1.0f), x => _fade.color = x, new Color(0, 0, 0, 0.0f), 1.0f).WaitForCompletion();

        EventManager.TriggerActivateCubeSequence();
        EventManager.OnEndSequence += EndNarrativeSequence;
        _endPortal.OpenDoor();
        yield return StartCoroutine(_cameraAnimator.TurnAround());
    }

    private void EndNarrativeSequence() => InputHandler.Instance.CanMove = true;

    [Button("Enable Rubik's Cube")]
    public void EnableRubiksCube() => ToggleRubiksCube(true);

    [Button("Disable Rubik's Cube")]
    public void DisableRubiksCube() => ToggleRubiksCube(false);
    
    private void ToggleRubiksCube(bool isEnabled)
    {
        isUIRubiksCubeEnabled = isEnabled;
    }

    public void MakeCutsceneLevelMarkAsFinished()
    {
        EventManager.TriggerLevelFinished();
    }
}
