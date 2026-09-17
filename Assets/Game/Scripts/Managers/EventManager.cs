using RubiksStatic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance => instance;
    private static EventManager instance;

    private GameSettings _gameSettings;

    public static bool gamePaused = false;

    public UnityEvent RotatingFace;

    public UnityEvent RotatingEnd;

    public UnityEvent SelectFace;
    public UnityEvent SwitchtRotation;

    public UnityEvent<float> Reset;
    public UnityEvent<float> Undo;

    public UnityEvent Interact;

    public static event Action<int> OnLevelSelected;


    private void Awake()
    {
        if (instance) Destroy(this);
        else instance = this;

        _gameSettings = GameManager.Instance.Settings;
        
    }

    private void OnEnable()
    {
        OnStartCubeRotation += RotatingFace.Invoke;
        OnEndCubeRotation += RotatingEnd.Invoke;
        
        OnCubeSwitchFace += SelectFace.Invoke;
        OnCubeSwitchAxe += SwitchtRotation.Invoke;

        OnPlayerReset += Reset.Invoke;
        OnPlayerUndo += Undo.Invoke;

        OnPlayerInteract += Interact.Invoke;


    }

    private void OnDisable()
    {
        OnStartCubeRotation -= RotatingFace.Invoke;
        OnEndCubeRotation -= RotatingEnd.Invoke;
        
        OnCubeSwitchFace -= SelectFace.Invoke;
        OnCubeSwitchAxe -= SwitchtRotation.Invoke;

        OnPlayerReset -= Reset.Invoke;
        OnPlayerUndo -= Undo.Invoke;

        OnPlayerInteract -= Interact.Invoke;
        
    }


    //Game Events
    public static event Action OnSceneStart;
    public static event Action OnSceneEnd;

    public static event Action OnLevelFinished;
    public static event Action OnPlayerLose;

    public static event Action OnSceneChange;

    public static event Action OnGamePause;
    public static event Action OnGameUnpause;



    public static event Action OnSeeExit;

    //Rubik's Cube Events
    public static event Action OnStartCubeRotation;

    public static event Action OnEndCubeRotation;
    public static event Action OnEndCubeSequenceRotation;

    public static event Action OnCubeSwitchFace;
    public static event Action OnCubeSwitchAxe;

    public static event Action OnPlayerTriesToRotateLockedTiles;

    //UI Events
    public static event Action<UIView> OnViewShow;
    public static event Action<UIView> OnViewHide;
    public static event Action OnEscape;


    //Object Events
    public static event Action OnButtonPressed;
    public static event Action OnButtonReleased;

    //Player Events
    public static event Action<float> OnPlayerReset;
    public static event Action<float> OnPlayerResetLose;

    public static event Action<float> OnPlayerUndo;
    public static event Action<RubiksMove> OnMoveReset;
    public static event Action OnPreviewCancel;

    public static event Action OnPlayerInteract;


    public static event Action OnActivateSequence;
    public static event Action OnEndSequence;

    public static event Action OnSkipNarraSequence;

    public static event Action OnPlayerChangeParent;
    public static event Action<GroundTypePlayerIsWalkingOn> OnPlayerFootSteps;

    public static event Action OnPlayerStopsFalling;

    public static event Action<bool> OnIsPlayerLookingAtInteractableObject;

    //Narrative Events
    public static event Action OnStartNarrativeSequence;
    public static event Action OnEndNarrativeSequence;
    public static event Action<bool> OnSkipCutscene;

    public static Delegate[] OnGamePauseCallStack => OnGamePause.GetInvocationList();
    public static Delegate[] OnGameUnpauseCallStack => OnGameUnpause.GetInvocationList();

    // Custom Settings Events
    public static event Action<float> OnFOVChange;
    public static event Action<float> OnMouseChange;
    public static event Action<bool> OnVibrationChange;
    public static event Action<bool> OnMotionBlurChange;
    public static event Action<bool> OnPreviewChange;




    public static void TriggerEscape()
    {
        OnEscape?.Invoke();
    }

    public static void TriggerViewShow(UIView uiView)
    {
        OnViewShow?.Invoke(uiView);
    }

    public static void TriggerViewHide(UIView uiView)
    {
        OnViewHide?.Invoke(uiView);
    }

    public void DebugTest()
    {
        Debug.Log("CACA -  Test r�ussi");
    }
    public static void TriggerCubeSwitchFace()
    {
        OnCubeSwitchFace?.Invoke();
    }
    public static void TriggerCubeSwitchAxe()
    {
        OnCubeSwitchAxe?.Invoke();
    }

    public static void TriggerPlayerTriesToRotateLockedTiles()
    {
        OnPlayerTriesToRotateLockedTiles?.Invoke();
    }

    public static void TriggerPlayerInteract()
    {
        OnPlayerInteract?.Invoke();
    }


    public static void TriggerLevelFinished()
    {
        OnLevelFinished?.Invoke();
    }

    public void TriggerPlayerLose()
    {
        OnPlayerLose?.Invoke();
        TriggerResetOnLose();
    }

    public static void TriggerSceneChange()
    {
        OnSceneChange?.Invoke();
    }

    public static void TriggerGamePause()
    {
        OnGamePause?.Invoke();
        gamePaused = true;

        // Pause audio
        if (AUDIO_ProgrammerInstrument.Instance != null)
            AUDIO_ProgrammerInstrument.Instance.Pause();
        
        foreach (var voice in FindObjectsOfType<AUDIO_CharacterVoice>())
            voice.Pause();
    }

    public static void TriggerGameUnpause()
    {
        OnGameUnpause?.Invoke();
        gamePaused = false;

        // Resume audio
        if (AUDIO_ProgrammerInstrument.Instance != null)
            AUDIO_ProgrammerInstrument.Instance.Resume();
        
        foreach (var voice in FindObjectsOfType<AUDIO_CharacterVoice>())
            voice.Resume();
    }
    public static void TriggerSeeExit()
    {
        OnSeeExit?.Invoke();
    }

    public static void TriggerButtonPressed()
    {
        OnButtonPressed?.Invoke();
    }

    public static void TriggerButtonReleased()
    {
        OnButtonReleased?.Invoke();
    }

    public static void TriggerLevelSelected(int levelIndex)
    {
        OnLevelSelected?.Invoke(levelIndex);
    }

    public static void TriggerPlayerChangeParent()
    {
        OnPlayerChangeParent?.Invoke();
    }

    public static void TriggerPlayerFootSteps(GroundTypePlayerIsWalkingOn _groundTypePlayerIsWalkingOn)
    {
        OnPlayerFootSteps?.Invoke(_groundTypePlayerIsWalkingOn);
    }

    public static void TriggerPlayerStopsFalling()
    {
        OnPlayerStopsFalling?.Invoke();
    }

    public static void TriggerIsPlayerLookingAtInteractableObject(bool isLooking)
    {
        OnIsPlayerLookingAtInteractableObject?.Invoke(isLooking);
    }

    public void TriggerResetOnLose()
    {
        float resetTime = _gameSettings.ResetCurve.Evaluate(GameManager.Instance.RubiksCube.Moves.Count);
        print("############# " + resetTime);
        OnPlayerResetLose?.Invoke(resetTime);
    }
    public void TriggerReset()
    {
        float resetTime = _gameSettings.ResetCurve.Evaluate(GameManager.Instance.RubiksCube.Moves.Count);
        OnPlayerReset?.Invoke(resetTime);
    }
    public void TriggerUndo()
    {
        OnPlayerUndo?.Invoke(_gameSettings.UndoDuration);
    }
    public static void TriggerMoveResetOnce(RubiksMove move)
    {
        OnMoveReset?.Invoke(move);
    }
    public static void TriggerPreviewCancel()
    {
        OnPreviewCancel?.Invoke();
    }

    public static void TriggerSceneStart()
    {
        OnSceneStart?.Invoke();
    }

    public static void TriggerSceneEnd()
    {
        OnSceneEnd?.Invoke();
    }

    public static void TriggerNarrativeSequenceStart()
    {
        OnStartNarrativeSequence?.Invoke();
    }

    public static void TriggerNarrativeSequenceEnd()
    {
        OnEndNarrativeSequence?.Invoke();
    }

    public static void TriggerStartCubeRotation()
    {
        OnStartCubeRotation?.Invoke();
    }

    public static void TriggerEndCubeSequenceRotation()
    {
        OnEndCubeSequenceRotation?.Invoke();
    }

    public static void TriggerEndCubeRotation()
    {
        OnEndCubeRotation?.Invoke();
    }

    public static void TriggerActivateCubeSequence()
    {
        OnActivateSequence?.Invoke();
    }

    public static void TriggerEndCubeSequence()
    {
        OnEndSequence?.Invoke();
    }

    public static void TriggerOnSkipCutscene(bool state)
    {
        OnSkipCutscene?.Invoke(state);
    }

    // Custom Settings Events

    public static void TriggerFOVChange(float newFOV)
    {
        OnFOVChange?.Invoke(newFOV);
    }
    public static void TriggerMouseChange(float newMouse)
    {
        OnMouseChange?.Invoke(newMouse);
    }
    public static void TriggerVibrationChange(bool newVibration)
    {
        OnVibrationChange?.Invoke(newVibration);
    }    
    public static void TriggerMotionBlurChange(bool newMotionBlur)
    {
        OnMotionBlurChange?.Invoke(newMotionBlur);
    }
    public static void TriggerPreviewChange(bool newPreview)
    {
        OnPreviewChange?.Invoke(newPreview);
    }
}
