using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.Windows;

public class SmoothDamping : MonoBehaviour
{
    [Header("Y-Axis")]

    // vertical movement
    [SerializeField] Transform _playerTransform;
    [SerializeField] float _verticalSmoothAmount;
    [SerializeField] float _verticalIntencity;
    [SerializeField] float _maxHight;
    [SerializeField] float _minHight;

    Vector3 _artCubeStartLocalPos;
    float _imaginaryCubePosY;
    float _verticalVelocity;

    [Header("X and Z Axis")]

    // horisontal movement
    [SerializeField] float _horizontalSmoothAmount;
    [SerializeField] float _horizontalIntencity;
    [SerializeField] float _maxHorizontalRange;
    [SerializeField] float _minHorizontalRange;

    float _imaginaryCubePosX;
    float _horizontalVelocityX;

    float _imaginaryCubePosZ;
    float _horizontalVelocityZ;

    [Header("Camera Rotation")]

    [SerializeField] Transform _cameraTransfrom;
    [SerializeField] float _rotationSmoothAmount;
    [SerializeField] float _rotationIntencity;
    [SerializeField] Vector2 _maxRange;
    [SerializeField] Vector2 _minRange;
    [SerializeField] Vector2 _maxHardClampRange;
    [SerializeField] Vector2 _minHardClampRange;

    Vector2 _imaginaryCubePosVec;
    Vector2 _rotationVelocity;
    Vector2 targetPos = Vector2.zero;

    [Header("Art Cube Rotation")]

    [SerializeField] float _cubeRotationIntencity;
    [SerializeField] float _maxCubeRotation;
    [SerializeField] bool LockRotationToWorldRotation;

    Quaternion _cubeStartRotation;
    MouseCamControl _mouseCamControl;

    private void Start()
    {
        _artCubeStartLocalPos = transform.localPosition;
        _imaginaryCubePosY = _playerTransform.position.y;
        _imaginaryCubePosX = _playerTransform.position.x;

        _cubeStartRotation = transform.parent.localRotation;
        _mouseCamControl = _cameraTransfrom.gameObject.GetComponent<MouseCamControl>();
    }

    private void LateUpdate()
    {
        // vertical movement ----------------

        // calculate _verticalVelocity
        float newImaginaryCubePos = Mathf.SmoothDamp(
            _imaginaryCubePosY,
            _playerTransform.position.y,
            ref _verticalVelocity,
            _verticalSmoothAmount
        );
        _imaginaryCubePosY = newImaginaryCubePos;

        // clamp
        if (_imaginaryCubePosY - _playerTransform.position.y > _maxHight) _imaginaryCubePosY = _playerTransform.position.y + _maxHight;
        if (_imaginaryCubePosY - _playerTransform.position.y < _minHight) _imaginaryCubePosY = _playerTransform.position.y + _minHight;


        // horizontal movement ----------------

        // X

        // calculate _horizontalVelocity
        newImaginaryCubePos = Mathf.SmoothDamp(
            _imaginaryCubePosX,
            _playerTransform.position.x,
            ref _horizontalVelocityX,
            _horizontalSmoothAmount
        );
        _imaginaryCubePosX = newImaginaryCubePos;

        // clamp
        if (_imaginaryCubePosX - _playerTransform.position.x > _maxHorizontalRange) _imaginaryCubePosX = _playerTransform.position.x + _maxHorizontalRange;
        if (_imaginaryCubePosX - _playerTransform.position.x < _minHorizontalRange) _imaginaryCubePosX = _playerTransform.position.x + _minHorizontalRange;


        // Z
        
        newImaginaryCubePos = Mathf.SmoothDamp(
            _imaginaryCubePosZ,
            _playerTransform.position.z,
            ref _horizontalVelocityZ,
            _horizontalSmoothAmount
        );
        _imaginaryCubePosZ = newImaginaryCubePos;

        // clamp
        if (_imaginaryCubePosZ - _playerTransform.position.z > _maxHorizontalRange) _imaginaryCubePosZ = _playerTransform.position.z + _maxHorizontalRange;
        if (_imaginaryCubePosZ - _playerTransform.position.z < _minHorizontalRange) _imaginaryCubePosZ = _playerTransform.position.z + _minHorizontalRange;


        // camera rotation movement ----------------

        targetPos += _mouseCamControl.MousePos;

        Vector2 newImaginaryCubePosVec = Vector2.SmoothDamp(
            _imaginaryCubePosVec,
            targetPos,
            ref _rotationVelocity,
            _rotationSmoothAmount
        );
        _imaginaryCubePosVec = newImaginaryCubePosVec;

        // clamp  - preserves smoothness but is relient on player movement speed. Bigger speed = wider clamp ranger. 
        if (_imaginaryCubePosVec.x - targetPos.x > _maxRange.x) _imaginaryCubePosVec.x = targetPos.x + _maxRange.x;
        if (_imaginaryCubePosVec.x - targetPos.x < _minRange.x) _imaginaryCubePosVec.x = targetPos.x + _minRange.x;

        if (_imaginaryCubePosVec.y - targetPos.y > _maxRange.y) _imaginaryCubePosVec.y = targetPos.y + _maxRange.y;
        if (_imaginaryCubePosVec.y - targetPos.y < _minRange.y) _imaginaryCubePosVec.y = targetPos.y + _minRange.y;

        // hardClamp  - This will look less mooth but will 100% prevent the cube from leaving the screen !
        if (_rotationVelocity.x > _maxHardClampRange.x) _rotationVelocity.x = _maxHardClampRange.x;
        if (_rotationVelocity.x < _minHardClampRange.x) _rotationVelocity.x = _minHardClampRange.x;

        if (_rotationVelocity.y > _maxHardClampRange.y) _rotationVelocity.y = _maxHardClampRange.y;
        if (_rotationVelocity.y < _minHardClampRange.y) _rotationVelocity.y = _minHardClampRange.y;

        Vector3 rotationVelocityV3 = new Vector3(_rotationVelocity.x, _rotationVelocity.y, 0);

        // create new pos ----------------
        transform.localPosition =
            _artCubeStartLocalPos

            + transform.InverseTransformDirection(Vector3.down) *
            _verticalVelocity *
            _verticalIntencity

            + transform.InverseTransformDirection(Vector3.left) *
            _horizontalVelocityX *
            _horizontalIntencity

            + transform.InverseTransformDirection(Vector3.back) *
            _horizontalVelocityZ *
            _horizontalIntencity

            - rotationVelocityV3 * _rotationIntencity;

        if (LockRotationToWorldRotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0f);
        }
        else
        {
            float yModifyer = (_rotationVelocity.y > _maxCubeRotation ? _maxCubeRotation : _rotationVelocity.y < _maxCubeRotation * -1 ? _maxCubeRotation * -1 : _rotationVelocity.y);
            float xModifyer = (_rotationVelocity.x > _maxCubeRotation ? _maxCubeRotation : _rotationVelocity.x < _maxCubeRotation * -1 ? _maxCubeRotation * -1 : _rotationVelocity.x);

            transform.parent.localRotation = _cubeStartRotation;
            transform.parent.Rotate(yModifyer * _cubeRotationIntencity, xModifyer * _cubeRotationIntencity * -1, 0f);
        }

        //transform.parent.localRotation = Quaternion.Euler(_rotationVelocity.y * _cubeRotationIntencity, _rotationVelocity.x * _cubeRotationIntencity * -1, 0f);

        //transform.parent.Rotate(_rotationVelocity.y * _cubeRotationIntencity, _rotationVelocity.x * _cubeRotationIntencity * -1, 0f);
    }
}
