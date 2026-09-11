using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChangeUiController : MonoBehaviour
{
    [SerializeField] bool _useGameObject = false; 
    
    [SerializeField] Sprite _Xbox;
    [SerializeField] Sprite _PS;
    [SerializeField, HideIf("_useGameObject")] Sprite _Keyboard;
    [SerializeField, ShowIf("_useGameObject")] GameObject _KeyboardGO;


    Image _sprite;
    private void Awake()
    {
        InputSystem.onActionChange += InputSystem_onDeviceChange;
        _sprite = GetComponent<Image>();


    }

    void OnDisable()
    {
        InputSystem.onActionChange -= InputSystem_onDeviceChange;
    }

    private void InputSystem_onDeviceChange(object obj, InputActionChange change)
    {
        InputAction receivedInputAction;
        InputDevice lastDevice;

        if (change == InputActionChange.ActionPerformed)
        {
            if(_KeyboardGO) _KeyboardGO.SetActive(false);
            _sprite.color = Color.white;
            
            receivedInputAction = (InputAction)obj;
            lastDevice = receivedInputAction.activeControl.device;

            string name = lastDevice.name;  


            if (name.Contains("XInputControllerWindows")) _sprite.sprite = _Xbox;
            else if (name.Contains("Playstation")|| name.Contains("DualSense")) _sprite.sprite = _PS;
            else
            {
                if(!_useGameObject) _sprite.sprite = _Keyboard;
                else
                {
                    if (!_KeyboardGO) return; //Debug.LogError(this.name + "Does Not Have GameObject");
                    _sprite.sprite = null;
                    _sprite.color = new Color32(0,0,0, 0);
                    _KeyboardGO.SetActive(true);
                }
            }
        }
    }

}
