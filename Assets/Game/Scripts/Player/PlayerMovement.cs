using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Scene Requirements")]
    [SerializeField] CharacterController _controller;
    [SerializeField] Transform _camera;
    [SerializeField] Transform _floorCheck;
    [SerializeField] LayerMask _floorLayer;

    bool _hasGravity = true;
    bool _FreeFallZone = false;

    [Header("Movement Modifiers")]
    [SerializeField, Range(0.0f, 2.0f)] float _speedMultiplier = 1.0f;

    [SerializeField, Min(0.0f)] private float _stairsSpeedMultiplier = 0.85f;

    [Header("Jump")]
    [SerializeField] bool _canJump = true;
    [SerializeField] float _floorDistance = 0.5f;
    [SerializeField] private float _coyoteTime;
    [SerializeField] float _maxPlayerFallSpeed = 50;

    [Header("Crouch")]
    [SerializeField] bool _canCrouch = true;

    [Header("Slipping")]
    [SerializeField][Range(0.0f, 0.1f)] float _slippingMovementControl = 0.01f;

    [Header("GravityRotation")]
    [SerializeField] bool _enableGravityRotation = true;

    [Header("NoClip")]
    [SerializeField] bool _resetRotationWhenNoClip = false;

    [Header("ViewBobbing")]
    [SerializeField] bool _isViewBobbingEnabled = true;

    bool _canMove = true;

    Vector3 _gravityDirection;

    private GroundTypePlayerIsWalkingOn _currentGroundType = GroundTypePlayerIsWalkingOn.Default;

    float _currentMoveSpeed;
    float _currentMoveSpeedFactor = 1f;
    Vector3 _verticalVelocity;
    float _currentFallSpeed;
    Vector3 _horizontalVelocity;
    bool _isGrounded;
    bool _oldIsGrounded;
    float _currentCoyoteTime;
    bool _isOnStairs;

    float _defaultCameraHeight;
    float _defaultControllerHeight;
    Vector3 _defaultControllerCenter;

    float _xInput = 0;
    float _zInput = 0;
    float _yInput = 0; //noclip
    bool _jumpInput = false;
    bool _crouchInput = false;

    bool _isSlipping = false;
    Vector3 _pastHorizontalVelocity;
    GameSettings _gameSettings;

    // HeadBobbing
    float _walkingDuration;
    float _startWalkingDuration;
    float _stopWalkingDuration;
    bool _isWalking;

    Vector3 newCamPos;

    Vector3 _externallyAppliedMovement = Vector3.zero;

    public bool isOnDefaultGround;

    bool _isUncontrolledFalling = false;

    public float defaultSpeed { get; private set; }
    public bool HasGravity { get => _hasGravity; set => _hasGravity = value; }
    public bool FreeFallZone { get => _FreeFallZone; set => _FreeFallZone = value; }

    private float _timerBeforeNextStep = 0;
    private PlayerStepDetection _stepDetection;
    public float _timerTNextStep = 1;
    private bool _isFirstFrame = true;

    private void OnEnable()
    {
        EventManager.OnEndCubeRotation += UnParentPlayer;
    }

    private void OnDisable()
    {
        EventManager.OnEndCubeRotation -= UnParentPlayer;
    }

    public void EnableMovement() => _canMove = true;
    public void DisableMovement() => _canMove = false;

    public void EnableBobbing() => _isViewBobbingEnabled = true;
    public void DisableBobbing() => _isViewBobbingEnabled = false;

    public void UnParentPlayer() => transform.SetParent(null);

    private void Awake()
    {
        _stepDetection = GetComponent<PlayerStepDetection>();
    }

    void Start()
    {
        _gameSettings = GameManager.Instance.Settings;
        GetComponent<DetectNewParent>().DoGravityRotation = _gameSettings.EnableGravityRotation;

        _defaultCameraHeight = _camera.transform.localPosition.y;
        _defaultControllerHeight = _controller.height;
        _defaultControllerCenter = _controller.center;

        defaultSpeed = _gameSettings.PlayerMoveSpeed * _speedMultiplier;
        _currentMoveSpeed = defaultSpeed;

        _currentCoyoteTime = _coyoteTime;
    }

    private void FixedUpdate()
    {
        //check player state
        _oldIsGrounded = _isGrounded;
        _isGrounded = Physics.CheckSphere(_floorCheck.position, _floorDistance, _floorLayer);

        if (!_canMove) return;

        if (_oldIsGrounded == false && _isGrounded == true)
        {
            // player just landed on the ground
            EventManager.TriggerPlayerStopsFalling();
            
            // Don't play landing sound on first frame (scene load)
            if (!_isFirstFrame)
            {
                _stepDetection.Land();
            }
        }
        
        // After first frame, allow landing sounds
        if (_isFirstFrame)
        {
            _isFirstFrame = false;
        }

        //apply gravity
        if (_hasGravity)
        {
            _gravityDirection = transform.up;

            //_verticalVelocity += _gravityDirection * (Math.Clamp(_gameSettings.Gravity * Time.deltaTime, 0, _maxPlayerFallSpeed));

            _currentFallSpeed += _gameSettings.Gravity * Time.deltaTime; // this is not used directly but helps track the current vertical velocity. 
            if (_currentFallSpeed > _maxPlayerFallSpeed * -1) // only add to the vertical velocity if fall speed is above the minimum vertical velocity. 
            {
                _verticalVelocity += _gravityDirection * (_gameSettings.Gravity * Time.deltaTime);
            }

            if (_isGrounded && _currentFallSpeed <= 0)
            {
                _currentFallSpeed = 0;
                _verticalVelocity = Vector3.zero;
            }
        }

        if (!_canMove) return;
        /*
        // collect player inputs
        _xInput = Input.GetAxis("Horizontal");
        _zInput = Input.GetAxis("Vertical");
        if (_canJump) _jumpInput = Input.GetButtonDown("Jump");
        if (_canCrouch) _crouchInput = Input.GetKey(KeyCode.LeftShift);
        */

        // movePlayer (walking around)
        if (_isGrounded)
        {
            _horizontalVelocity = _camera.right * _xInput + _camera.forward * _zInput;

        }
        else
        {
            float mag = _horizontalVelocity.magnitude;
            if (mag != 0) _horizontalVelocity = (_camera.right * _xInput + _camera.forward * _zInput).normalized * mag * 1.002f;
            else _horizontalVelocity = (_camera.right * _xInput + _camera.forward * _zInput);
        }

        if (_isSlipping)
        {
            _horizontalVelocity = _horizontalVelocity * _gameSettings.SlippingMovementControl + _pastHorizontalVelocity;

            //clamp
            _horizontalVelocity.x = _horizontalVelocity.x > 1 ? 1 : _horizontalVelocity.x;
            _horizontalVelocity.x = _horizontalVelocity.x < -1 ? -1 : _horizontalVelocity.x;
            _horizontalVelocity.z = _horizontalVelocity.z > 1 ? 1 : _horizontalVelocity.z;
            _horizontalVelocity.z = _horizontalVelocity.z < -1 ? -1 : _horizontalVelocity.z;
        }

        if (_isGrounded)
            _currentCoyoteTime = _coyoteTime;
        else
            _currentCoyoteTime -= Time.deltaTime;

        // jump
        if (_jumpInput && (_isGrounded || _currentCoyoteTime > 0f)) {
            _verticalVelocity = transform.up * Mathf.Sqrt(_gameSettings.MaxJumpHeight * -2f * _gameSettings.Gravity);
            _currentCoyoteTime = -1.0f;
            _horizontalVelocity = _pastHorizontalVelocity * 1.1f;
            _stepDetection.Jump();
        }

        if (_isGrounded && !_jumpInput)
        {
            _verticalVelocity = Vector3.zero - new Vector3(0.0f, 9f, 0.0f);
        }

        _jumpInput = false;

        // crouch
        if (_crouchInput) {
            _controller.height *= _gameSettings.CrouchHeight;
            _controller.center = Vector3.up * _gameSettings.CrouchHeight * -1;

            newCamPos = _camera.transform.localPosition;
            newCamPos.y = _defaultCameraHeight * _gameSettings.CrouchHeight;
        } else {
            _controller.height = _defaultControllerHeight;
            _controller.center = _defaultControllerCenter;
            newCamPos = _camera.transform.localPosition;
            newCamPos.y = _defaultCameraHeight;
        }

        // no clip
        if (_FreeFallZone == false)
            _horizontalVelocity += transform.up * _yInput;
        else
            _horizontalVelocity += transform.up * .95f;

        if (_isGrounded && _isUncontrolledFalling) _isUncontrolledFalling = false;
        if (_isUncontrolledFalling) _horizontalVelocity = Vector3.zero; //cancel any non-vertical movement

        _isOnStairs = false;
        if (Physics.Raycast(transform.position, -transform.up, out var hit, 10000, LayerMask.GetMask("Floor")))
        {
            if (hit.normal != Vector3.up)
            {
                _isOnStairs = true;
            }
        }
    }

    private void Update()
    {
        if (_canMove)
        {
            // apply calculated Movement
            float moveSpeed = _currentMoveSpeed * _currentMoveSpeedFactor * (_isOnStairs ? _stairsSpeedMultiplier : 1);
            if (_hasGravity) {
                _controller.Move((_horizontalVelocity * Time.deltaTime * ((_crouchInput ? moveSpeed : moveSpeed / _gameSettings.CrouchSpeed)) + _externallyAppliedMovement) * (!_isGrounded ? _gameSettings.AirControl : 1.0f));
                _controller.Move(_verticalVelocity * Time.deltaTime);
            } else // no clip
            {
                _controller.Move(_horizontalVelocity * ((moveSpeed / 10) * Time.deltaTime)
                                 + _externallyAppliedMovement);
            }
        }

        _crouchInput = false;
        _pastHorizontalVelocity = _horizontalVelocity;
        ExecuteFootStep();
    }

    private void LateUpdate()
    {
        if (_isViewBobbingEnabled)
        {
            _ApplyCameraHeight(newCamPos.y);
        }
    }

    void ExecuteFootStep()
    {
        if (!_isGrounded) return;

        if (_isWalking) {
            _timerBeforeNextStep += Time.deltaTime;
            EventManager.TriggerPlayerFootSteps(_currentGroundType);

        }
        else {
            _timerBeforeNextStep = 0;
        }

        float stepDuration = _timerTNextStep / _currentMoveSpeedFactor;
        if (_timerBeforeNextStep >= stepDuration) {
            _timerBeforeNextStep = 0;
            UpdateGroundType();
            EventManager.TriggerPlayerFootSteps(_currentGroundType);

        }
    }

    #region Inputs

    public void ActionMovement(Vector2 direction)
    {
        //Debug.Log("actionMovement direction is " + direction);
        _xInput = direction.x;
        _zInput = direction.y;
    }

    public void ActionJump()
    {
        _jumpInput = _canJump;
    }

    public void ActionCrouch()
    {
        _crouchInput = _canCrouch;
    }

    #endregion

    public void SetSpeed(float newSpeed)
    {
        _currentMoveSpeed = newSpeed * Time.deltaTime;
    }

    public void SetSpeedFactor(float speedFactor)
    {
        _currentMoveSpeedFactor = speedFactor;
    }

    public void SetSpeedToDefault()
    {
        _currentMoveSpeed = defaultSpeed;
    }

    public void SetSlippingState(bool isSlipping)
    {
        _isSlipping = isSlipping;
    }

    private void _ApplyCameraHeight(float currentDefaultHeight)
    {
        Vector3 newCameraHeight;
        _isWalking = _horizontalVelocity != Vector3.zero;
        if (_isWalking && !_isSlipping) {
            if (_startWalkingDuration <= _gameSettings.StartWalkingTransitionDuration) {
                _stopWalkingDuration = 0.0f;
                _startWalkingDuration += Time.deltaTime;
                newCameraHeight = Vector3.up * (_gameSettings.ViewBobbingWalkMultiplier * Mathf.Lerp(_camera.transform.localPosition.y,
                    currentDefaultHeight + _gameSettings.HeadBobbingCurve.Evaluate(0.0f) * _gameSettings.HeadBobbingAmount,
                    _startWalkingDuration / _gameSettings.StartWalkingTransitionDuration));
            } else {
                _walkingDuration += Time.deltaTime;

                if (Physics.Raycast(transform.position, -transform.up, out var hit, 10000, LayerMask.GetMask("Floor")))
                {
                    if (hit.normal != Vector3.up)
                    {
                        newCameraHeight = Vector3.up * (_gameSettings.ViewBobbingStairsMultiplier * (currentDefaultHeight + _gameSettings.HeadBobbingStairsCurve.Evaluate((_walkingDuration * _gameSettings.HeadBobbingSpeed) % 1) * _gameSettings.HeadBobbingAmount));
                    }
                    else
                    {
                        newCameraHeight = Vector3.up * (_gameSettings.ViewBobbingWalkMultiplier * (currentDefaultHeight + _gameSettings.HeadBobbingCurve.Evaluate((_walkingDuration * _gameSettings.HeadBobbingSpeed) % 1) * _gameSettings.HeadBobbingAmount));
                    }
                }
                else
                {
                    newCameraHeight = Vector3.up * (_gameSettings.ViewBobbingWalkMultiplier * (currentDefaultHeight + _gameSettings.HeadBobbingCurve.Evaluate((_walkingDuration * _gameSettings.HeadBobbingSpeed) % 1) * _gameSettings.HeadBobbingAmount));
                }
            }
        } else {
            _walkingDuration = 0.0f;
            if (_stopWalkingDuration <= _gameSettings.StopWalkingTransitionDuration) {
                _startWalkingDuration = 0.0f;
                _stopWalkingDuration += Time.deltaTime;
                newCameraHeight = Vector3.up * Mathf.Lerp(_camera.transform.localPosition.y,
                    currentDefaultHeight,
                    _stopWalkingDuration / _gameSettings.StopWalkingTransitionDuration);
            } else {
                newCameraHeight = Vector3.up * currentDefaultHeight;
            }
        }
        float cameraHightModifyer = newCameraHeight.y - _camera.transform.localPosition.y;
        _camera.transform.localPosition += cameraHightModifyer * Vector3.up;
    }

    //NoClip
    public void ActivateNoClip()
    {
        GetComponent<CharacterController>().excludeLayers = Physics.AllLayers;
        _hasGravity = false;
        _verticalVelocity = Vector3.zero;
        _controller.Move(Vector3.zero);
        transform.SetParent(null);
        if (_resetRotationWhenNoClip) {
            transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        }
    }

    public void DeactivateNoClip()
    {
        GetComponent<CharacterController>().excludeLayers = 0;
        _hasGravity = true;
    }

    public void ActionVerticalMovement(float direction)
    {
        _yInput = direction;
    }

    public void SetExternallyAppliedMovement(Vector3 directon, float speed = 1)
    {
        _externallyAppliedMovement = directon * speed;
    }

    private void UpdateGroundType()
    {
        Ray ray = new Ray(_floorCheck.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, _floorDistance + 0.2f, _floorLayer))
        {
            string groundTag = hit.collider.tag;
            //Debug.Log("Ground tag detected: " + groundTag);
            switch (groundTag)
            {
                case "Floor_Default":
                default:
                    _currentGroundType = GroundTypePlayerIsWalkingOn.Default;
                    break;
                case "Floor_Grass":
                    _currentGroundType = GroundTypePlayerIsWalkingOn.Grass;
                    break;
            }
            //Debug.Log("Ground type detected: " + groundTag);
        }
        else
        {
            //Debug.Log("No ground or tag detected , setting to default.");
            _currentGroundType = GroundTypePlayerIsWalkingOn.Default;
        }
    }

    // player will have locked movement until they stop falling (until _isGrounded == true). 
    public void SetUncontrolledFalling(bool isUncontrolledFalling) { _isUncontrolledFalling = isUncontrolledFalling;}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_floorCheck.position, _floorDistance);
    }
}