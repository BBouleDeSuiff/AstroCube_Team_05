using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Scene Requirements")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _floorCheck;
    [SerializeField] private LayerMask _floorLayer;

    [Header("Movement Modifiers")]
    [SerializeField, Range(0.0f, 2.0f)] private float _speedMultiplier = 1.0f;
    [SerializeField, Min(0.0f)] private float _stairsSpeedMultiplier = 0.85f;

    [Header("Jump & Fall")]
    [SerializeField] private bool _canJump = true;
    [SerializeField] private float _floorDistance = 0.3f;
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _maxPlayerFallSpeed = 50f;
    [SerializeField] private float _jumpCooldownTimer = 0f;

    [Header("Crouch")]
    [SerializeField] private bool _canCrouch = true;

    [Header("Slipping")]
    [SerializeField, Range(0.0f, 0.1f)] private float _slippingMovementControl = 0.01f;

    [Header("View Bobbing")]
    [SerializeField] private bool _isViewBobbingEnabled = true;

    [Header("NoClip")]
    [SerializeField] private bool _resetRotationWhenNoClip = false;

    private bool _hasGravity = true;
    private bool _freeFallZone = false;
    private bool _canMove = true;
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isSlipping;
    private bool _isOnStairs;
    private bool _isUncontrolledFalling;
    private bool _isFirstFrame = true;

    private float _currentMoveSpeed;
    private float _currentMoveSpeedFactor = 1f;
    private float _currentCoyoteTime;
    private float _currentFallSpeed;
    private Vector3 _verticalVelocity;
    private Vector3 _horizontalVelocity;
    private Vector3 _pastHorizontalVelocity;
    private Vector3 _externallyAppliedMovement;

    private float _defaultCameraHeight;
    private float _defaultControllerHeight;
    private Vector3 _defaultControllerCenter;
    private Vector3 _targetCamPos;

    private float _xInput;
    private float _zInput;
    private float _yInput;
    private bool _jumpRequested;
    private bool _crouchHeld;

    private float _walkingDuration;
    private float _startWalkingDuration;
    private float _stopWalkingDuration;
    private float _timerBeforeNextStep;
    public float _timerTNextStep = 1f;
    private GroundTypePlayerIsWalkingOn _currentGroundType = GroundTypePlayerIsWalkingOn.Default;

    private PlayerStepDetection _stepDetection;
    private GameSettings _gameSettings;

    public float DefaultSpeed { get; private set; }
    public bool HasGravity { get => _hasGravity; set => _hasGravity = value; }
    public bool FreeFallZone { get => _freeFallZone; set => _freeFallZone = value; }

    private void Awake()
    {
        if (!_controller) _controller = GetComponent<CharacterController>();
        _stepDetection = GetComponent<PlayerStepDetection>();
    }

    private void OnEnable() => EventManager.OnEndCubeRotation += UnParentPlayer;
    private void OnDisable() => EventManager.OnEndCubeRotation -= UnParentPlayer;

    private void Start()
    {
        _gameSettings = GameManager.Instance.Settings;

        /*if (TryGetComponent<DetectNewParent>(out var detectParent))
            detectParent.DoGravityRotation = _gameSettings.EnableGravityRotation;*/

        _defaultCameraHeight = _camera.localPosition.y;
        _defaultControllerHeight = _controller.height;
        _defaultControllerCenter = _controller.center;
        _targetCamPos = _camera.localPosition;

        DefaultSpeed = _gameSettings.PlayerMoveSpeed * _speedMultiplier;
        _currentMoveSpeed = DefaultSpeed;
    }

    private void Update()
    {
        CheckGroundStatus();

        if (_canMove)
        {
            HandleCrouch();
            HandleHorizontalMovement();
            HandleVerticalMovement();
            ApplyFinalMovement();
        }

        ExecuteFootStep();
        _pastHorizontalVelocity = _horizontalVelocity;
    }

    private void LateUpdate()
    {
        if (_isViewBobbingEnabled)
            ApplyCameraHeight(_targetCamPos.y);
    }

    private void CheckGroundStatus()
    {
        _wasGrounded = _isGrounded;

        
        if (_jumpCooldownTimer > 0f)
        {
            _jumpCooldownTimer -= Time.deltaTime;
            _isGrounded = false; 
        }
        else
        {
            _isGrounded = Physics.CheckSphere(_floorCheck.position, _floorDistance, _floorLayer, QueryTriggerInteraction.Ignore);
        }

        if (!_wasGrounded && _isGrounded)
        {
            EventManager.TriggerPlayerStopsFalling();
            if (!_isFirstFrame && _stepDetection != null)
                _stepDetection.Land();

            _isUncontrolledFalling = false;
        }

        if (_isFirstFrame) _isFirstFrame = false;

        
        _isOnStairs = false;
        if (Physics.Raycast(transform.position, -transform.up, out var hit, 2.5f, _floorLayer))
        {
            if (Vector3.Angle(hit.normal, transform.up) > 5f && Vector3.Angle(hit.normal, transform.up) < 60f)
                _isOnStairs = true;
        }
    }

    private void HandleHorizontalMovement()
    {
        Vector3 inputDir = (_camera.right * _xInput + _camera.forward * _zInput);
        inputDir = Vector3.ProjectOnPlane(inputDir, transform.up).normalized;

        if (_isGrounded)
        {
            _horizontalVelocity = inputDir;
        }
        else
        {
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, inputDir, _gameSettings.AirControl * Time.deltaTime * 5f);
        }

        if (_isSlipping)
        {
            _horizontalVelocity = Vector3.ClampMagnitude(
                _horizontalVelocity * _gameSettings.SlippingMovementControl + _pastHorizontalVelocity, 1.0f);
        }

        if (_isUncontrolledFalling)
            _horizontalVelocity = Vector3.zero;

        if (!_hasGravity)
        {
            _horizontalVelocity += transform.up * (_freeFallZone ? 0.95f : _yInput);
        }
    }

    private void HandleVerticalMovement()
    {

        if (!_hasGravity)
        {
            _verticalVelocity = Vector3.zero;
            _currentFallSpeed = 0;
            return;
        }

        if (_isGrounded)
        {
            _currentCoyoteTime = _coyoteTime;

            if (!_jumpRequested)
                _verticalVelocity = -transform.up * 4f;
        }
        else
        {
            _currentCoyoteTime -= Time.deltaTime;
        }

        if (_jumpRequested && _canJump && (_isGrounded || _currentCoyoteTime > 0f))
        {
            float jumpForce = Mathf.Sqrt(_gameSettings.MaxJumpHeight * 2f * Mathf.Abs(_gameSettings.Gravity));
            _verticalVelocity = transform.up * jumpForce;
            _currentCoyoteTime = 0f;
            _jumpCooldownTimer = 0.2f; // <-- Prevents ground check from triggering for 200ms
            _isGrounded = false;      

            if (_stepDetection != null) _stepDetection.Jump();
        }
        _jumpRequested = false;

        if (!_isGrounded)
        {
            _currentFallSpeed += _gameSettings.Gravity * Time.deltaTime;
            _currentFallSpeed = Mathf.Clamp(_currentFallSpeed, -_maxPlayerFallSpeed, _maxPlayerFallSpeed);
            _verticalVelocity += transform.up * (_gameSettings.Gravity * Time.deltaTime);
        }
        else if (_verticalVelocity.y < 0 && Vector3.Dot(_verticalVelocity, transform.up) < 0)
        {
            _currentFallSpeed = 0f;
        }
    }

    private void ApplyFinalMovement()
    {
        float speed = _currentMoveSpeed * _currentMoveSpeedFactor * (_isOnStairs ? _stairsSpeedMultiplier : 1f);
        if (_crouchHeld) speed *= _gameSettings.CrouchSpeed;

        Vector3 moveDelta = (_horizontalVelocity * speed + _externallyAppliedMovement) * Time.deltaTime;
        moveDelta += _verticalVelocity * Time.deltaTime;

        _controller.Move(moveDelta);
    }

    private void HandleCrouch()
    {
        if (!_canCrouch) return;

        float targetHeight = _crouchHeld ? (_defaultControllerHeight * _gameSettings.CrouchHeight) : _defaultControllerHeight;
        Vector3 targetCenter = _crouchHeld ? (Vector3.up * (_gameSettings.CrouchHeight * -0.5f)) : _defaultControllerCenter;
        float targetCamY = _crouchHeld ? (_defaultCameraHeight * _gameSettings.CrouchHeight) : _defaultCameraHeight;

        _controller.height = targetHeight;
        _controller.center = targetCenter;
        _targetCamPos.y = targetCamY;
    }

    private void ExecuteFootStep()
    {
        if (!_isGrounded || _horizontalVelocity.sqrMagnitude < 0.01f)
        {
            _timerBeforeNextStep = 0f;
            return;
        }

        _timerBeforeNextStep += Time.deltaTime;
        float stepDuration = _timerTNextStep / Mathf.Max(_currentMoveSpeedFactor, 0.1f);

        if (_timerBeforeNextStep >= stepDuration)
        {
            _timerBeforeNextStep = 0f;
            UpdateGroundType();
            EventManager.TriggerPlayerFootSteps(_currentGroundType);
        }
    }

    private void UpdateGroundType()
    {
        if (Physics.Raycast(_floorCheck.position, -transform.up, out RaycastHit hit, _floorDistance + 0.3f, _floorLayer))
        {
            _currentGroundType = hit.collider.CompareTag("Floor_Grass")
                ? GroundTypePlayerIsWalkingOn.Grass
                : GroundTypePlayerIsWalkingOn.Default;
        }
        else
        {
            _currentGroundType = GroundTypePlayerIsWalkingOn.Default;
        }
    }

    private void ApplyCameraHeight(float baseHeight)
    {
        bool isMoving = _horizontalVelocity.sqrMagnitude > 0.01f && !_isSlipping && _isGrounded;

        if (isMoving)
        {
            _stopWalkingDuration = 0f;
            _startWalkingDuration += Time.deltaTime;
            _walkingDuration += Time.deltaTime;

            var curve = _isOnStairs ? _gameSettings.HeadBobbingStairsCurve : _gameSettings.HeadBobbingCurve;
            float multiplier = _isOnStairs ? _gameSettings.ViewBobbingStairsMultiplier : _gameSettings.ViewBobbingWalkMultiplier;
            float bobOffset = curve.Evaluate((_walkingDuration * _gameSettings.HeadBobbingSpeed) % 1f) * _gameSettings.HeadBobbingAmount;

            float blend = Mathf.Clamp01(_startWalkingDuration / _gameSettings.StartWalkingTransitionDuration);
            float targetY = Mathf.Lerp(_camera.localPosition.y, baseHeight + (bobOffset * multiplier), blend);

            Vector3 pos = _camera.localPosition;
            pos.y = targetY;
            _camera.localPosition = pos;
        }
        else
        {
            _walkingDuration = 0f;
            _startWalkingDuration = 0f;
            _stopWalkingDuration += Time.deltaTime;

            float blend = Mathf.Clamp01(_stopWalkingDuration / _gameSettings.StopWalkingTransitionDuration);
            Vector3 pos = _camera.localPosition;
            pos.y = Mathf.Lerp(pos.y, baseHeight, blend);
            _camera.localPosition = pos;
        }
    }

    #region Input Handlers

    public void ActionMovement(Vector2 direction)
    {
        _xInput = direction.x;
        _zInput = direction.y;
    }

    public void SetExternallyAppliedMovement(Vector3 direction, float speed = 1f)
    {
        _externallyAppliedMovement = direction * speed;
    }

    public void ActionJump() => _jumpRequested = true;
    public void ActionCrouch(bool isCrouching) => _crouchHeld = isCrouching;
    public void ActionVerticalMovement(float direction) => _yInput = direction;

    #endregion

    public void SetSpeed(float newSpeed) => _currentMoveSpeed = newSpeed;
    public void SetSpeedFactor(float speedFactor) => _currentMoveSpeedFactor = speedFactor;
    public void SetSpeedToDefault() => _currentMoveSpeed = DefaultSpeed;
    public void SetSlippingState(bool isSlipping) => _isSlipping = isSlipping;
    public void SetUncontrolledFalling(bool isFalling) => _isUncontrolledFalling = isFalling;
    public void EnableMovement() => _canMove = true;
    public void DisableMovement() => _canMove = false;
    public void EnableBobbing() => _isViewBobbingEnabled = true;
    public void DisableBobbing() => _isViewBobbingEnabled = false;
    public void UnParentPlayer() => transform.SetParent(null);

    public void ActivateNoClip()
    {
        _controller.excludeLayers = Physics.AllLayers;
        _hasGravity = false;
        _verticalVelocity = Vector3.zero;
        transform.SetParent(null);

        if (_resetRotationWhenNoClip)
            transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
    }

    public void DeactivateNoClip()
    {
        _controller.excludeLayers = 0;
        _hasGravity = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (_floorCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_floorCheck.position, _floorDistance);
        }
    }
}