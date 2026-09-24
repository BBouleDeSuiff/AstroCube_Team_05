using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystemManager;

public class CursorLockState : MonoBehaviour
{
    public enum EInputMode
    {
        KEYBOARD,
        CONTROLLER,
    }
    EInputMode _currentInputMode;

    private void Awake()
    {
        //InputSystem.onActionChange += InputActionChangeCallback;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LockMouse(false);
    }

    void LockMouse(bool enabled)
    {
        if (InputSystemManager.Instance.CurrentInputMode == InputSystemManager.EInputMode.KEYBOARD)
        {
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        } else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    //private void InputActionChangeCallback(object obj, InputActionChange change)
    //{
    //    if (change == InputActionChange.ActionPerformed)
    //    {
    //        InputAction receivedInputAction = (InputAction)obj;
    //        InputDevice lastDevice = receivedInputAction.activeControl.device;

    //        var tempInputMode = _currentInputMode;
    //        _currentInputMode = lastDevice.name.Equals("Keyboard") || lastDevice.name.Equals("Mouse") ? EInputMode.KEYBOARD : EInputMode.CONTROLLER;

    //        if (tempInputMode != _currentInputMode)
    //        {
    //            Cursor.lockState = _currentInputMode == EInputMode.KEYBOARD ? CursorLockMode.None : CursorLockMode.Locked;
    //        }
    //    }

    //    LockMouse(false);
    //}
}
