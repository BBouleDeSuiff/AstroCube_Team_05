using System;
using RubiksStatic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RubiksCubeController : MonoBehaviour
{
    [SerializeField] GameObject _controlledCube;
    RubiksMovement _controlledScript;
    RubiksMove _lastInput = null;
    bool _isPreviewDisplayed;

    bool _canPlayerUseIt = true;


    [SerializeField] RubiksMovement _previewControlledScript;


    bool _cameraPlayerReversed = false;

    [SerializeField] Transform _overlayTransform;
    bool _isCameraRotating = false;

    [SerializeField] List<GameObject> ReplicatedCube = new List<GameObject>();

    [SerializeField] bool _ShowStripLayerToPlayer = true;

    List<RubiksMovement> _replicatedScript = new List<RubiksMovement>();

    [SerializeField] Transform _player;
    [SerializeField] DetectNewParent _detectParentForGroundRotation;

    [SerializeField] SliceAxis _selectedSlice = 0;
    private GameSettings _gameSettings;

    bool _canPlayerMoveAxis = true;
    bool _doesCurrentAxisHaveLockedTile;
    int _numOfLockedTileRotationAttempts = 0;

    ArtAnimatorSync ArtCubeSync;
    public Dictionary<int, Transform> replicatedMap = new();

    #region Accesseur

    public bool CameraPlayerReversed { get => _cameraPlayerReversed; set => _cameraPlayerReversed = value; }
    public bool ShowStripLayerToPlayer { get => _ShowStripLayerToPlayer; set => _ShowStripLayerToPlayer = value; }
    public RubiksMovement ControlledScript { get => _controlledScript; }
    public SliceAxis SelectedSlice => _selectedSlice;
    [field: SerializeField] public SelectionCube ActualFace { get; private set; }
    public bool CanPlayerUseIt { get => _canPlayerUseIt; set => _canPlayerUseIt = value; }

    #endregion

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _detectParentForGroundRotation = _player.GetComponentInChildren<DetectNewParent>();
        if (_controlledCube != null) _controlledScript = _controlledCube.GetComponentInChildren<RubiksMovement>();
        foreach (GameObject go in ReplicatedCube)
        {
            if (go)
                _replicatedScript.Add(go.GetComponentInChildren<RubiksMovement>());
        }

        for (int i = 0; i < 26; i++)
        {
            replicatedMap.Add(_controlledScript.AllBlocks[i].gameObject.GetInstanceID(), _replicatedScript[0].AllBlocks.Find(x => x.localPosition == _controlledScript.AllBlocks[i].localPosition));
        }

        ArtCubeSync = FindAnyObjectByType<ArtAnimatorSync>();

        _gameSettings = GameManager.Instance.Settings;
    }
    private void Start()
    {
        if (_previewControlledScript == null)
        {
            var previewScript = GameObject.FindAnyObjectByType<PreviewRubiksCube>();
            if (previewScript)
                _previewControlledScript = previewScript.GetComponentInChildren<RubiksMovement>();
        }
        if (_previewControlledScript)
            _HidePreview();
    }

    private void OnEnable()
    {
        EventManager.OnPlayerReset += ResetPreview;
        EventManager.OnPlayerUndo += ResetPreview;
        EventManager.OnPreviewCancel += ResetPreview;
        EventManager.OnPlayerChangeParent += CheckPreview;

    }
    private void OnDisable()
    {
        EventManager.OnPlayerReset -= ResetPreview;
        EventManager.OnPlayerUndo -= ResetPreview;
        EventManager.OnPreviewCancel -= ResetPreview;
        EventManager.OnPlayerChangeParent -= CheckPreview;
    }

    public void ReMapArtCube()
    {
        replicatedMap.Clear();
        for (int i = 0; i < 26; i++)
        {
            replicatedMap.Add(_controlledScript.AllBlocks[i].gameObject.GetInstanceID(), _replicatedScript[0].AllBlocks.Find(x => x.localPosition == _controlledScript.AllBlocks[i].localPosition));
        }
    }

    public void SetActualCube(Transform newFace)
    {
        _ShutDownFace();
        if (newFace.parent.parent && newFace.parent.parent.CompareTag("Rubiks")) _controlledCube = newFace.parent.parent.gameObject;
        else if (newFace.parent.CompareTag("Rubiks")) _controlledCube = newFace.parent.gameObject;

        _controlledScript = _controlledCube.GetComponentInChildren<RubiksMovement>();

        if (_controlledScript == null) return;
        if (_controlledScript.IsRotating) return;
        GameManager.Instance.RubiksCube = _controlledScript;
        if (ActualFace) ActualFace.enabled = false;
        ActualFace = newFace.GetComponent<SelectionCube>();

        if (ControlledScript.IsTransformInside(_player) && _ShowStripLayerToPlayer && _TryIlluminateFace(_selectedSlice, SelectionCube.SelectionMode.AXIS))
        {
            if (!(_previewControlledScript && _isPreviewDisplayed && GameManager.Instance.CustomSettings.customPreview))
                ActualFace.Select(SelectionCube.SelectionMode.CUBE);
            _canPlayerMoveAxis = true;
        }
        else
        {
            _canPlayerMoveAxis = false;
        }

        _controlledScript.GetAxisFromCube(ActualFace.transform, _selectedSlice);
    }

    public void ActionSwitchLineCols(bool isLeft, bool playEvent = true)
    {
        EventManager.TriggerCubeSwitchAxe();
        _selectedSlice = (SliceAxis)(((int)_selectedSlice + (isLeft ? -1 : +1) + 3) % 3);
        GameManager.Instance.ActualSliceAxis = _selectedSlice;
        switch (_selectedSlice)
        {
            case SliceAxis.X:
                _detectParentForGroundRotation.ToggleParentChanger(false);
                if (_controlledScript.IsLockXAxis)
                {
                    _player.SetParent(null);
                    ActionSwitchLineCols(true);
                    return;
                }
                break;
            case SliceAxis.Y:
                _detectParentForGroundRotation.ToggleParentChanger(true);
                if (_controlledScript.IsLockYAxis)
                {
                    ActionSwitchLineCols(true);
                    return;
                }
                break;
            case SliceAxis.Z:
                _detectParentForGroundRotation.ToggleParentChanger(false);
                if (_controlledScript.IsLockZAxis)
                {
                    _player.SetParent(null);
                    ActionSwitchLineCols(true);
                    return;
                }
                break;
        }


        foreach (var go in _replicatedScript[0].AllBlocks)
        {
            go.GetComponentInChildren<ArtRubiksAnimator>()?.SetSelectedBool(false);
        }


        if (ActualFace) SetActualCube(ActualFace.transform);
    }
    private void LateUpdate()
    {
        foreach (var go in _replicatedScript[0].AllBlocks)
        {
            go.GetComponentInChildren<ArtRubiksAnimator>()?.SetSelectedBool(false);
        }
    }
    public void ActionMakeTurn(bool clockwise)
    {
        #region Locked tiles audio event
        if (_doesCurrentAxisHaveLockedTile && _numOfLockedTileRotationAttempts < 11)
        {
            _numOfLockedTileRotationAttempts++;
            if (_numOfLockedTileRotationAttempts == 2 || _numOfLockedTileRotationAttempts == 5 || _numOfLockedTileRotationAttempts == 10)
            {
                EventManager.TriggerPlayerTriesToRotateLockedTiles();
            }
        }
        #endregion

        if (_controlledScript && !_controlledScript.IsRotating && ControlledScript.IsTransformInside(_player))
        {
            if (_player.GetComponent<PlayerTrigger>().IsPlayerInLockRotationZone) return;

            if (!_canPlayerUseIt) return;

            if (!_canPlayerMoveAxis && (!_previewControlledScript || !_isPreviewDisplayed))
            {
                RubiksMove failedInput = new()
                {
                    Axis = _controlledScript.GetAxisFromCube(ActualFace.transform, _selectedSlice),
                    cube = ActualFace.transform,
                    orientation = _selectedSlice,
                    clockWise = clockwise
                };
                
                _controlledScript.RotateAxisFailed(failedInput.Axis, failedInput.cube, clockwise, 1f, failedInput.orientation);
                return;
            }
            if (_previewControlledScript && _previewControlledScript.IsRotating)
                return;
            if (_cameraPlayerReversed)
            {
                clockwise = !clockwise;
            }

            //ShutDownFace();
            //_ShineSelection(_selectedSlice, SelectionCube.SelectionMode.AXIS);

            if (_controlledScript == null) return;
            if (ActualFace == null) return;

            RubiksMove input = new()
            {
                Axis = _controlledScript.GetAxisFromCube(ActualFace.transform, _selectedSlice),
                cube = ActualFace.transform,
                orientation = _selectedSlice,
                clockWise = clockwise
            };

            if (_previewControlledScript)
            {
                bool completeAction = false;

                if (_lastInput != null)
                {
                    if (_lastInput.cube == null)
                    {
                        // Should this happen ?
                    }
                    else
                    {
                        //bool isSameFace = _controlledScript.GetCubesFromFace(_lastInput.cube, _lastInput.orientation).Contains(input.cube);
                        //completeAction = isSameFace && _lastInput == input;
                        completeAction = _lastInput.clockWise == input.clockWise;
                    }
                }

                if (!completeAction)
                {
                    if (_isPreviewDisplayed)
                    {
                        _previewControlledScript.UndoMove(0.0f);
                        Transform equivalence = _previewControlledScript.transform.GetComponentsInChildren<Transform>().First(t => t.GetComponentIndex() == _lastInput.cube.transform.GetComponentIndex());
                        _previewControlledScript.RotateAxis(_previewControlledScript.GetAxisFromCube(equivalence, _lastInput.orientation), _lastInput.cube.transform, clockwise, _gameSettings.PreviewRubikscCubeAxisRotationDuration, _lastInput.orientation);
                        _lastInput.clockWise = clockwise;

                    }
                    else
                    {
                        _HidePreview();
                        _ShowPreview(_selectedSlice, SelectionCube.SelectionMode.AXIS);
                        Transform equivalence = _previewControlledScript.transform.GetComponentsInChildren<Transform>().First(t => t.GetComponentIndex() == ActualFace.transform.GetComponentIndex());
                        _previewControlledScript.RotateAxis(_previewControlledScript.GetAxisFromCube(equivalence, _selectedSlice), ActualFace.transform, clockwise, _gameSettings.PreviewRubikscCubeAxisRotationDuration, _selectedSlice);
                        _lastInput = input;
                    }

                    _isPreviewDisplayed = true;
                }
                else
                {
                    foreach (RubiksMovement cube in _replicatedScript)
                    {
                        if (!cube) continue;

                        Transform equivalence = cube.transform.GetChild(_lastInput.cube.transform.GetComponentIndex());
                        // Get The index of the children 
                        // Find the Other child at the index in other cube
                        // Move it

                        cube.RotateAxis(cube.GetAxisFromCube(equivalence, _lastInput.orientation), _lastInput.cube.transform, clockwise, _gameSettings.RubikscCubeAxisRotationDuration, _lastInput.orientation);
                    }
                    _HidePreview();
                    _ShineSelection(_lastInput.orientation, SelectionCube.SelectionMode.AXIS, _lastInput.cube.transform);
                    _ExteriorCollidersActivation(_lastInput.orientation, _lastInput.cube.transform);
                    _controlledScript.RotateAxis(_controlledScript.GetAxisFromCube(_lastInput.cube.transform, _lastInput.orientation), _lastInput.cube.transform, clockwise, _gameSettings.RubikscCubeAxisRotationDuration, _lastInput.orientation);
                    _lastInput = null;
                    _isPreviewDisplayed = false;
                }
            }
            else
            {
                foreach (RubiksMovement cube in _replicatedScript)
                {
                    if (!cube) continue;

                    Transform equivalence = cube.transform.GetChild(ActualFace.transform.GetComponentIndex());
                    // Get The index of the children 
                    // Find the Other child at the index in other cube
                    // Move it
                    cube.RotateAxis(cube.GetAxisFromCube(equivalence, _selectedSlice), ActualFace.transform, clockwise, _gameSettings.RubikscCubeAxisRotationDuration, _selectedSlice);
                }

                _ShineSelection(_selectedSlice, SelectionCube.SelectionMode.AXIS, ActualFace.transform);
                _ExteriorCollidersActivation(_selectedSlice, ActualFace.transform);
                _controlledScript.RotateAxis(_controlledScript.GetAxisFromCube(ActualFace.transform, _selectedSlice), ActualFace.transform, clockwise, _gameSettings.RubikscCubeAxisRotationDuration, _selectedSlice);
            }
        }
    }

    private void ResetPreview(float duration) => ResetPreview();
    private void ResetPreview()
    {
        if (_isPreviewDisplayed && _previewControlledScript)
        {
            _previewControlledScript.UndoMove(0.0f);
            _HidePreview();
            _lastInput = null;
            _isPreviewDisplayed = false;
        }
    }

    void CheckPreview()
    {
        List<SelectionCube> selectionCubes = new List<SelectionCube>();
        bool isOneTileLocked = false;
        bool isPlayerOnATile = false;
        if (_controlledScript != null && _lastInput != null && _lastInput.cube != null)
        {
            foreach (Transform go in _controlledScript.GetCubesFromFace(_lastInput.cube.transform, _lastInput.orientation))
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;


                selectionCubes.Add(selection);
                if (selection.IsTileLocked) isOneTileLocked = true;
                if (_detectParentForGroundRotation.CurrentParent == selection && _selectedSlice != SliceAxis.Y) isPlayerOnATile = true;
            }
        }
        if (_previewControlledScript && _isPreviewDisplayed && (isPlayerOnATile || isOneTileLocked))
            ResetPreview();
    }

    public void ActionRotateCubeUI(Vector2 direction)
    {
        StartCoroutine(RotateCubeUI(direction));
    }

    IEnumerator RotateCubeUI(Vector2 direction)
    {
        if (!_isCameraRotating)
        {

            _isCameraRotating = true;
            float elapsedTime = 0;
            Quaternion startRotation = _overlayTransform.rotation;
            Quaternion targetRotation = startRotation * Quaternion.AngleAxis(-90, new Vector2(direction.y, direction.x));

            while (elapsedTime < _gameSettings.UiRubikscCubeRotationDuration)
            {
                elapsedTime += Time.deltaTime;
                _overlayTransform.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / _gameSettings.UiRubikscCubeRotationDuration);
                yield return null;
            }

            _overlayTransform.transform.rotation = targetRotation;

            _isCameraRotating = false;
        }
    }

    public bool IsPlayerOnTile(SliceAxis sliceAxis, Transform cube)
    {
        if (_controlledScript != null)
        {
            foreach (Transform go in _controlledScript.GetCubesFromFace(cube, sliceAxis))
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;

                if (_detectParentForGroundRotation.CurrentParent == selection && sliceAxis != SliceAxis.Y)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /* OLD
    void IlluminateFace(SliceAxis sliceAxis)
    {
        Color hey = new Color(1, 0.5f, 0, 1);
        if(_controlledScript != null)
        foreach (Transform go in _controlledScript.GetCubesFromFace(ActualFace.transform, sliceAxis))   
        {
            Outline outline = go.GetComponent<Outline>();
            outline.OutlineColor = hey;
            outline.OutlineWidth = 10;
            outline.enabled = true;
        }
    */

    List<Transform> OldBlocksInFace = new List<Transform>();

    /// <summary>
    /// return True if Player Can Move Axis;
    /// </summary>
    /// <param name="sliceAxis"></param>
    /// <param name="mode">Should normally never be set to Locked</param>
    /// <returns></returns>
    bool _TryIlluminateFace(SliceAxis sliceAxis, SelectionCube.SelectionMode mode)
    {
        if (_player.GetComponent<PlayerTrigger>().IsPlayerInLockRotationZone) return false;

        if (!GameManager.Instance.IsUIRubiksCubeEnabled)
            return false;

        List<SelectionCube> selectionCubes = new List<SelectionCube>();
        bool isOneTileLocked = false;
        bool isPlayerOnATile = false;
        if (_controlledScript != null)
        {
            if (_controlledScript.IsRotating || _controlledScript.IsReversing) return false;


            var AllBlocksInFace = _controlledScript.GetCubesFromFace(ActualFace.transform, sliceAxis);
            bool isSameBlock = OldBlocksInFace == AllBlocksInFace;
            OldBlocksInFace = AllBlocksInFace;

            foreach (Transform go in AllBlocksInFace)
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;


                selectionCubes.Add(selection);
                if (selection.IsTileLocked) isOneTileLocked = true;
                if (_detectParentForGroundRotation.CurrentParent == selection && sliceAxis != SliceAxis.Y) isPlayerOnATile = true;
            }


            var previousFace = new List<Transform>(oldReplicatedFace);

            List<Animator> animToSpeedUp = new();
            float amountToSpeed = 0;
            Animator otherAnim = null;
            
            if(!isPlayerOnATile && !isOneTileLocked)
            oldReplicatedFace.Clear();
            
            
            bool doOnce = true;
            for (int i = 0; i < 9; i++)
            {
                if (isPlayerOnATile) break;
                if (isOneTileLocked) break;

                var objectID = AllBlocksInFace[i].gameObject.GetInstanceID();

                if (replicatedMap.TryGetValue(objectID, out var obj))
                {
                    var anim = obj.GetComponentInChildren<ArtRubiksAnimator>();

                    if (previousFace.Contains(obj))
                    {
                        animToSpeedUp.Add(anim.animatorCube);
                    }
                    else
                    {
                        otherAnim = anim.animatorCube;
                    }


                    if (doOnce && animToSpeedUp.Count > 0 && !animToSpeedUp.Contains(anim.animatorCube))
                    {
                        doOnce = false;
                        //amountToSpeed = ArtCubeSync.CalculateMultValue(anim.animatorCube,animToSpeedUp[0]);
                    }

                    anim?.SetSelectedBool(true);

                    oldReplicatedFace.Add(obj);
                }
            }

            if (animToSpeedUp.Count > 0 && animToSpeedUp.Count < 4)
            {
                StartCoroutine(ArtCubeSync.ChangeAnimatorSpeeds(animToSpeedUp, otherAnim));
            }

        }

        _doesCurrentAxisHaveLockedTile = isOneTileLocked;

        if (_previewControlledScript && _isPreviewDisplayed && GameManager.Instance.CustomSettings.customPreview)
            return !(isPlayerOnATile || isOneTileLocked);

        foreach (SelectionCube selection in selectionCubes)
        {
            if (isPlayerOnATile)
            {
                selection.Select(SelectionCube.SelectionMode.PLAYERONTILE);
            }
            else if (isOneTileLocked)
            {
                selection.Select(SelectionCube.SelectionMode.LOCKED);
            }
            else
            {
                selection.Select(mode);
            }
        }
        return !(isPlayerOnATile || isOneTileLocked);
    }

    List<Transform> oldReplicatedFace = new();

    void _ShutDownFace()
    {
        foreach (Transform go in _controlledCube.transform)
        {
            SelectionCube selection = go.GetComponent<SelectionCube>();
            if (selection == null) continue;
            selection.Unselect();
        }
    }

    void _ShineSelection(SliceAxis sliceAxis, SelectionCube.SelectionMode mode, Transform actualFace)
    {
        if (_controlledScript != null)
        {
            foreach (Transform go in _controlledScript.GetCubesFromFace(actualFace, sliceAxis))
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;
                selection.StartShineAnim();
            }
        }
    }

    void _ExteriorCollidersActivation(SliceAxis sliceAxis, Transform actualFace)
    {
        if (sliceAxis == SliceAxis.Y) return; // if player is snanding on the ground while it rotates. 

        if (_controlledScript != null)
        {
            foreach (Transform go in _controlledScript.GetCubesFromFace(actualFace, sliceAxis))
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;
                selection.StartActivateExteriorColliders();
            }
        }
    }

    // Creared variation of _ExteriorCollidersActivation to be used in the FollowSequence() function of the RubiksMovement script. 
    // Am I making spagettie? Maybe. Yum! Yum! [italian noises]. 
    public static void ExteriorCollidersActivation(SliceAxis sliceAxis, Transform actualFace, RubiksMovement controlledScript, float durationInSeconds)
    {
        if (sliceAxis == SliceAxis.Y) return; // if player is snanding on the ground while it rotates. 

        if (controlledScript != null)
        {
            foreach (Transform go in controlledScript.GetCubesFromFace(actualFace, sliceAxis))
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;
                selection.StartActivateExteriorCollidersWithDuration(durationInSeconds);
            }
        }
    }

    void _ShowPreview(SliceAxis sliceAxis, SelectionCube.SelectionMode mode)
    {
        if (_previewControlledScript != null)
        {
            foreach (Transform go in _previewControlledScript.GetCubesFromFace(ActualFace.transform, sliceAxis))
            {
                SelectionCube selection = go.GetComponent<SelectionCube>();
                if (selection == null) continue;
                selection.Select(SelectionCube.SelectionMode.ENABLE);
            }
        }
    }

    void _HidePreview()
    {
        SelectionCube[] selectionCubes = _previewControlledScript.transform.parent.GetComponentsInChildren<SelectionCube>();
        foreach (SelectionCube selection in selectionCubes)
        {
            selection.Select(SelectionCube.SelectionMode.DISABLE);
        }
    }
}


