using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameActionPlaySound : AGameAction
{
    [SerializeField] private FMODUnity.EventReference _soundEvent;

    public override string BuildGameObjectName()
    {
        return "PLAY SOUND";

    }

    protected override void ExecuteSpecific()
    {
        if (!_soundEvent.IsNull)
            FMODUnity.RuntimeManager.PlayOneShot(_soundEvent, transform.position);
    }

}
