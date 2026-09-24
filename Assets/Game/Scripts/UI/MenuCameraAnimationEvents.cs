using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraAnimationEvents : MonoBehaviour
{
    [SerializeField] ToggleMenuElements _togMenuElem;

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    public void DeactivateTitleScreenAnimEvent()
    {
        if (_togMenuElem != null) _togMenuElem.Deactivate(MenuElement.TITLE_SCREEN);
    }

    public void ActivateStartMenuAnimEvent()
    {
        if (_togMenuElem != null) _togMenuElem.Activate(MenuElement.START_MENU);
        //Cursor.lockState = CursorLockMode.None;
    }
}
