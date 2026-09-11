using System;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class ActivateControlTips : MonoBehaviour
{
    [SerializeField] GameObject _restartControlTipPopUp;
    [SerializeField] int _numOfRotationsBeforPopUp;
    [SerializeField] float _amountOfTimeBeforPopUp;
    float _timeKeeper;

    // triggers with 'true' when the suggestion is activated (made visible) and 'false' when the suggestion is diactivated (made invisible). 
    [SerializeField] UnityEvent<bool> SuggestRestartEvent;
    static event Action<bool> OnSuggestRestart;

    int _rotationCount = 0;

    private void Start()
    {
        if (_restartControlTipPopUp == null)
        {
            Debug.Log("No ControlTip GameObject assigned to ActivateControlTips script.");
            return;
        }
        if (_restartControlTipPopUp.activeSelf) _restartControlTipPopUp.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.OnEndCubeRotation += Add1ToRotationCount;
        EventManager.OnPlayerReset += ResetRotationCound;
        OnSuggestRestart += SuggestRestartEvent.Invoke;
    }

    private void OnDisable()
    {
        EventManager.OnEndCubeRotation -= Add1ToRotationCount;
        EventManager.OnPlayerReset -= ResetRotationCound;
        OnSuggestRestart -= SuggestRestartEvent.Invoke;
    }

    private void Update()
    {
        _timeKeeper += Time.deltaTime;

        if (_timeKeeper >= _amountOfTimeBeforPopUp)
        {
            OnSuggestRestart(true);
            _timeKeeper = 0;
        }
    }

    void Add1ToRotationCount()
    {
        if (_restartControlTipPopUp == null) return;

        _rotationCount++;
        if (_rotationCount >= _numOfRotationsBeforPopUp) OnSuggestRestart.Invoke(true);
    }

    void ResetRotationCound(float iDontNeedThisFloat)
    {
        if (_restartControlTipPopUp == null) return;

        _rotationCount *= -1;
        OnSuggestRestart.Invoke(false);
    }

    public void SetPlaceHolderPopUpActive(bool newActiveStatus)
    {
        if (_restartControlTipPopUp.activeSelf == !newActiveStatus)
            _restartControlTipPopUp.SetActive(newActiveStatus);
    }
}
