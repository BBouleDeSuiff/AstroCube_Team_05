using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameActionToggleInput : AGameAction
{
    private enum EInputToggleType
    {
        ENABLE,
        DISABLE
    }

    [SerializeField] private EInputToggleType _inputToggleType = EInputToggleType.ENABLE;
    [EnumFlags]
    [SerializeField] private InputSystemManager.EInputType _targetInputs;

    public override string BuildGameObjectName()
    {
        return $"{_inputToggleType.ToString()} INPUTS";
    }

    protected override void ExecuteSpecific()
    {
        switch (_inputToggleType)
        {
            case EInputToggleType.ENABLE:
                InputHandler.EnableInputs(GetFlags(_targetInputs));
                break;  
            case EInputToggleType.DISABLE:
                InputHandler.DisableInputs(GetFlags(_targetInputs));
                break;
        }
    }

    private List<InputSystemManager.EInputType> GetFlags(InputSystemManager.EInputType e)
    {
        return Enum.GetValues(e.GetType()).Cast<InputSystemManager.EInputType>().Where(v => !Equals((int)(object)v, 0) && e.HasFlag(v)).ToList();
    }

}
