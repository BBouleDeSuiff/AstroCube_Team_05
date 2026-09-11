using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoBehaviour
{
    public static InputSystemManager Instance => instance;
    private static InputSystemManager instance;

    public EInputMode CurrentInputMode => _currentInputMode;
    private EInputMode _currentInputMode;

    public UnityEvent<EInputMode> OnCurrentInputModeChange;

    public PlayerInput PlayerInputs => _playerInputs;
    private PlayerInput _playerInputs;

    public enum EInputMode
    {
        CONTROLLER,
        KEYBOARD
    }    
    
    public enum EInputType
    {

        NONE = 0,
        CLOCKWISE = 1 << 0,
        COUNTER_CLOCKWISE = 1 << 1,
        SWITCH_COLUMNS_LINE_LEFT = 1 << 2,
        SWITCH_COLUMNS_LINE_RIGHT = 1 << 3,
        RESET_ROOM = 1 << 4,
        MOVE_OVERLAY_CUBE = 1 << 5,
        SHOW_STRIPS = 1 << 6,
        GAME_PAUSE = 1 << 7,
        INTERACT = 1 << 8,
        PAUSE_GAME = 1 << 9,
        MOVEMENT = 1 << 10,
        CAMERA = 1 << 11,
        SEE_EXIT = 1 << 12,
        PREVIEW_CANCEL = 1 << 13,
        JUMP = 1 << 14, 
        SKIP_NARRA = 1 << 15,
    }

    private void Awake()
    {
        if (instance) Destroy(this);
        else instance = this;

        _playerInputs = GetComponent<PlayerInput>();
        InputSystem.onActionChange += InputActionChangeCallback;
        OnCurrentInputModeChange.RemoveAllListeners();
    }

    private void InputActionChangeCallback(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction receivedInputAction = (InputAction)obj;
            InputDevice lastDevice = receivedInputAction.activeControl.device;

            var tempInputMode = _currentInputMode;
            _currentInputMode = lastDevice.name.Equals("Keyboard") || lastDevice.name.Equals("Mouse") ? EInputMode.KEYBOARD : EInputMode.CONTROLLER;
            
            if (tempInputMode != _currentInputMode)
            {
                OnCurrentInputModeChange?.Invoke(_currentInputMode);
                Cursor.lockState = _currentInputMode == EInputMode.KEYBOARD? CursorLockMode.None : CursorLockMode.Locked;
            }
        }
    }

    public InputAction GetInputActionFromName(string name) => _playerInputs.actions.FindAction(name);
    public string GetNameFromType(EInputType type)
    {
        switch (type) {
            default:
            case EInputType.CLOCKWISE:
                return("ClockWise");
            case EInputType.COUNTER_CLOCKWISE:
                return ("CounterClockWise");
            case EInputType.SWITCH_COLUMNS_LINE_LEFT:
                return ("SwitchColumnsLineLeft");
            case EInputType.SWITCH_COLUMNS_LINE_RIGHT:
                return("SwitchColumnsLineRight");
            case EInputType.RESET_ROOM:
                return("ResetRoom");
            case EInputType.MOVE_OVERLAY_CUBE:
                return("MoveOverlayCube");
            case EInputType.SHOW_STRIPS:
                return("ShowStrips");
            case EInputType.GAME_PAUSE:
                return("GamePause");
            case EInputType.INTERACT:
                return("Interact");
            case EInputType.PAUSE_GAME:
                return("PauseGame");
            case EInputType.MOVEMENT:
                return("Movement");
            case EInputType.CAMERA:
                return("Camera");
            case EInputType.SEE_EXIT:
                return("SeeExit");          
            case EInputType.PREVIEW_CANCEL:
                return("PreviewCancel");
            case EInputType.JUMP:
                return ("Jump");            
            case EInputType.SKIP_NARRA:
                return ("SkipCutscene");
        }
    }

    private void OnEnable()
    {
        EventManager.OnGamePause += DeactivateActionMap;
        EventManager.OnGameUnpause += ActivateActionMap;
    }

    private void OnDisable()
    {
        EventManager.OnGamePause -= DeactivateActionMap;
        EventManager.OnGameUnpause -= ActivateActionMap;
    }

    void DeactivateActionMap()
    {
        InputHandler.DisableInputs(new[]{EInputType.JUMP, EInputType.MOVEMENT}.ToList());
    }    
    void ActivateActionMap()
    {
        InputHandler.EnableInputs(new[]{EInputType.JUMP, EInputType.MOVEMENT}.ToList());
    }
    
}
