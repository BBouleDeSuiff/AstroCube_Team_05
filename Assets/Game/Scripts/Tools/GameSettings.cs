using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public string playthoughDataFileName { get; set; }

    // Player Properties
    public float PlayerMoveSpeed => _playerMoveSpeed;
    public float StepHeightMax => _stepHeightMax;
    public float Gravity => _gravity;
    public float MaxJumpHeight => _maxJumpHeight;
    public float AirControl => _airControl;
    public float CrouchSpeed => _crouchSpeed;
    public float CrouchHeight => _crouchHeight;
    public float SlippingMovementControl => _slippingMovementControl;
    public float HeadBobbingAmount => _headBobbingAmount;
    public float HeadBobbingSpeed => _headBobbingSpeed;
    public float StartWalkingTransitionDuration => _startWalkingTransitionDuration;
    public float StopWalkingTransitionDuration => _stopWalkingTransitionDuration;
    public AnimationCurve HeadBobbingCurve => _headBobbingCurve;
    public AnimationCurve HeadBobbingStairsCurve => _headBobbingStairsCurve;
    public float ViewBobbingWalkMultiplier => viewBobbingWalkMultiplier;
    public float ViewBobbingStairsMultiplier => viewBobbingStairsMultiplier;
    public bool EnableGravityRotation => _enableGravityRotation;


    // Rubik's Cube Properties
    public float RubikscCubeAxisRotationDuration => _rubikscCubeAxisRotationDuration;
    public float PreviewRubikscCubeAxisRotationDuration => _previewRubikscCubeAxisRotationDuration;
    public float UiRubikscCubeRotationDuration => _uiRubikscCubeRotationDuration;
    public bool AimAtObject => _aimAtObject;

    public AnimationCurve AnimationSpeedCurve => _AnimationSpeedCurve;
    public Vector4 RubiksEndCubeRotationScreenshakeSettings => _rubiksEndCubeRotationscreenshakeSettings;
    public Vector4 RubiksStartCubeRotationScreenshakeSettings => _rubiksStartCubeRotationscreenshakeSettings;

    // Visual Cues Properties
    public float AxisSelectionFadeInDuration => _axisSelectionFadeInDuration;
    public float AxisSelectionFadeOutDuration => _axisSelectionFadeOutDuration;

    // Global Properties
    public AnimationCurve ResetCurve => _resetCurve;
    public float UndoDuration => _undoDuration;
    public float StencilFadeInDuration => _stencilFadeInDuration;
    public float StencilFadeOutDuration => _stencilFadeOutDuration;
    public float StencilStayDuration => _stencilStayDuration;
    
    //Cutscene Properties
    public float SkipCutsceneDuration => _skipCutsceneDuration;
    
    // Achievements
    public float MaxTimeForAchievement => _maxTimeForAchievement;
    public int MaxRotationsForAchievement => _maxRotationsForAchievement;
    public int MaxLockedRotationsForAchievement => _maxLockedRotationsForAchievement;


    public AnimationCurve CurveFOV => curveFOV;
    public AnimationCurve CurveAberration => curveAberration;

    public AnimationCurve C_MIN => C_Min;
    public AnimationCurve FailedRotationCurve => _failedRotationCurve;
    public int MaxMemoryObjectsForAchievement => _maxMemoryObjectsForAchievement;

    [Header("-- PLAYER --")]

    [Header("- Movement")]
    [SerializeField] float _playerMoveSpeed = 12f;
    [SerializeField] float _stepHeightMax = 12f;
    [SerializeField] float _gravity = -20.0f;

    [FormerlySerializedAs("_jumpHeight")]
    [Header("- Jump")]
    [SerializeField] float _maxJumpHeight = 1.0f;
    [SerializeField, Range(0f, 1f)] float _airControl = 1.0f;

    [Header("- Crouch")]
    [SerializeField, Range(0.0f, 1.0f)] float _crouchSpeed = 0.5f;
    [SerializeField, Range(0.0f, 1.0f)] float _crouchHeight = 0.5f;

    [Header("- Slipping")]
    [SerializeField][Range(0.0f, 0.1f)] float _slippingMovementControl = 0.01f;
    
    [Header("- HeadBobbing -")]
    [SerializeField] float _headBobbingAmount = 2.0f;
    [SerializeField] float _headBobbingSpeed = 3.0f;
    [SerializeField] float _startWalkingTransitionDuration = 0.5f;
    [SerializeField] float _stopWalkingTransitionDuration = 0.5f;
    
    [Header("- HeadBobbing Walking")]
    [SerializeField] private float viewBobbingWalkMultiplier;
    [SerializeField] AnimationCurve _headBobbingCurve;

    [Header("- HeadBobbing Stairs")]
    [SerializeField] private float viewBobbingStairsMultiplier;
    [SerializeField] AnimationCurve _headBobbingStairsCurve;

    [Header("- GravityRotation")]
    [SerializeField] bool _enableGravityRotation = false;

    [Header("-- RUBIK'S CUBE --")]
    [SerializeField] private float _rubikscCubeAxisRotationDuration = 0.2f;
    [SerializeField] private float _previewRubikscCubeAxisRotationDuration = 0.2f;
    [SerializeField] private float _uiRubikscCubeRotationDuration = 0.2f;
    [SerializeField] private bool _aimAtObject = false;

    [SerializeField] AnimationCurve _AnimationSpeedCurve = new AnimationCurve();
    [SerializeField] AnimationCurve _failedRotationCurve = new AnimationCurve();

    [SerializeField][InfoBox("Duration, Strength, Vibrato, Randomness", EInfoBoxType.Normal)] 
    private Vector4 _rubiksEndCubeRotationscreenshakeSettings = new(2.0f, 0.4f, 10.0f, 90.0f);
    [SerializeField] private Vector4 _rubiksStartCubeRotationscreenshakeSettings = new(2.0f, 0.4f, 10.0f, 90.0f);

    [Header("-- VISUAL CUES--")]
    [SerializeField] private float _axisSelectionFadeInDuration = 0.6f;
    [SerializeField] private float _axisSelectionFadeOutDuration = 0.6f;

    [Header("-- GLOBAL --")] 
    [SerializeField] private AnimationCurve _resetCurve;
    [SerializeField] private float _undoDuration = 2.0f;


    [Header("- Show Exit")]
    [SerializeField] float _stencilFadeInDuration = 0.5f;
    [SerializeField] float _stencilFadeOutDuration = 0.5f;
    [SerializeField] float _stencilStayDuration = 2.0f;

    [SerializeField] AnimationCurve curveFOV;
    [SerializeField] AnimationCurve curveAberration;
    [SerializeField] AnimationCurve C_Min;

    [Header("- Cutscene Duration")] [SerializeField]
    private float _skipCutsceneDuration;

    [Header("-- ACHIEVEMENTS --")]
    [SerializeField] private float _maxTimeForAchievement;
    [SerializeField] private int _maxRotationsForAchievement;
    [SerializeField] private int _maxLockedRotationsForAchievement;
    [SerializeField] private int _maxMemoryObjectsForAchievement;
}
