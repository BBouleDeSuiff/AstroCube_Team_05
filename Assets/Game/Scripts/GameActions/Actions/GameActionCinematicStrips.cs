using UnityEngine;

public class GameActionCinematicStrips : AGameAction
{
    [SerializeField] private bool _state;
    [SerializeField] private float _animationDuration;

    protected override void ExecuteSpecific()
    {
        // LocalizationManager.Instance.SetStrips(_state, _animationDuration);         <--- commented for no-ui build
    }

    public override string BuildGameObjectName()
    {
        return $"{(_state ? "ENABLE" : "DISABLE")} CINEMATIC STRIPS";
    }
}