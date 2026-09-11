using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using FMODUnity;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineVirtualCamera vcam;
    [SerializeField] Camera overlayCamera;

    [SerializeField] CameraFocusAttractor cameraFocusAttractor;
    [SerializeField] float valueThatTriggersCamPan = 0.2f;

    [Header("SpeedZone")]
    [SerializeField] float newSpeedMultiplyer = 0.5f;

    [SerializeField] VolumeProfile vol;

    GameSettings _gameSettings;
    PlayerMovement _playerMovement;
    CharacterController _characterController;

    FloatingZone _flotingZone;

    [SerializeField] private Material portailInt_Material;
    [SerializeField] float fovMultiplier = 1.0f;

    [Header("FMOD")]
    [SerializeField] private EventReference _victoryZoneEvent;

    private bool isInExitFocusState = false;
    private Coroutine _fovCoroutine;

    private Reseter _reset;

    public  bool IsPlayerInLockRotationZone { get; private set; }


    private void Awake()
    {
        if (vcam == null) Debug.LogWarning("Cinemachine Virtual Camera not found.");
        if (overlayCamera == null) Debug.LogWarning("Overlay Camera not found.");
        if (cameraFocusAttractor == null) Debug.LogWarning("CameraFocusAttractor not found.");
    }

    private void Start()
    {
        _gameSettings = GameManager.Instance.Settings;

        if (portailInt_Material == null)
            Debug.LogError("C_Min_Material is not assigned.");

        if (!vol)
            vol = GameObject.FindGameObjectWithTag("GlobalVol")?.GetComponent<VolumeProfile>();

        if (vol && vol.TryGet<ChromaticAberration>(out var ca))
            ca.intensity.Override(.1f);

        _playerMovement = GetComponent<PlayerMovement>();
        _characterController = GetComponent<CharacterController>();

        portailInt_Material.SetFloat("_C_Min", _gameSettings.C_MIN.Evaluate(1));
        if (vcam)
            vcam.m_Lens.FieldOfView = GameManager.Instance.CustomSettings.customFov;

        _reset = GetComponent<Reseter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "VictoryZone":
                EventManager.TriggerLevelFinished();

                if (!_victoryZoneEvent.IsNull) RuntimeManager.PlayOneShot(_victoryZoneEvent);
                break;

            case "DeathZone":
                EventManager.Instance.TriggerPlayerLose();
                break;

            case "SlipperyZone":
                _playerMovement.SetSlippingState(true);
                break;

            case "SpeedZone":
                _playerMovement.SetSpeed(_playerMovement.defaultSpeed * newSpeedMultiplyer);
                break;

            case "GravityZone":
                _playerMovement.HasGravity = false;
                _flotingZone = other.transform.GetComponent<FloatingZone>();
                break;
            case "FreeFallZone":
                _playerMovement.FreeFallZone = true;
                break;
            case "ChangeReset":
                _reset.ChangeResetFunc(other.GetComponentInChildren<ChangeReset>().NewResetPos);
                break;
            case "ObjectLoader":
                other.GetComponent<ObjectLoader>().SwitchActivate();
                break;

            case "UncontrolledFallingTrigger": // to be triggered when the player starts falling though the menger sponge fractal. 
                _playerMovement.SetUncontrolledFalling(true);
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_flotingZone && other.CompareTag("GravityZone"))
            _characterController.Move(Vector3.up * _flotingZone.GravityForce * Time.deltaTime);

        if (other.CompareTag("ConveyerBelt"))
        {
            var belt = other.GetComponent<ConveyerBeltManager>();
            _playerMovement.SetExternallyAppliedMovement(belt.direction, belt.speed);
        }

        if (other.CompareTag("LockAllCubeRotationZone"))
        {
            IsPlayerInLockRotationZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case "SlipperyZone":
                _playerMovement.SetSlippingState(false);
                break;

            case "SpeedZone":
                _playerMovement.SetSpeedToDefault();
                break;

            case "GravityZone":
                _flotingZone = null;
                _playerMovement.HasGravity = true;
                break;

            case "ConveyerBelt":
                _playerMovement.SetExternallyAppliedMovement(Vector3.zero);
                break;
            case "FreeFallZone":
                _playerMovement.FreeFallZone = false;
                break;

            case "LockAllCubeRotationZone":
                IsPlayerInLockRotationZone = false;
                break;
        }
    }

    void SmoothCameraTransition(float targetFOV, float duration)
    {
        if (_fovCoroutine != null)
            StopCoroutine(_fovCoroutine);

        _fovCoroutine = StartCoroutine(TransitionCameraFOV(targetFOV, duration));
    }

    IEnumerator TransitionCameraFOV(float targetFOV, float duration)
    {
        float startFOV = vcam.m_Lens.FieldOfView;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            vcam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        vcam.m_Lens.FieldOfView = targetFOV;
        _fovCoroutine = null;
    }

    // Deactivates CromaticAberration filter when exiting playmode. 
    private void OnApplicationQuit()
    {
        if (vol && vol.TryGet<ChromaticAberration>(out var ca))
        {
            ca.intensity.Override(0f);
        }
    }
}
