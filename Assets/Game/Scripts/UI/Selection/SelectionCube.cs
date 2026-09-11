//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

using DG.Tweening;
using DG.Tweening.Core;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

[DisallowMultipleComponent]
public class SelectionCube : MonoBehaviour
{
    [SerializeField] bool _isTileLocked;
    [SerializeField] Material _lockedTileMat;
    [SerializeField] Material _lockedObjectMat;
    [SerializeField] AnimationCurve _bizmuthShineCurve;
    /*
    [SerializeField]
    int _defaultRenderingLayerMask, _cubeObjectSelectionRenderingLayerMask = 9, _axisObjectSelectionRenderingLayerMask = 10, _cubeSelectionRenderingLayerMask, _axisSelectionRenderingLayerMask, _axisLockRenderingLayerMask = 6, _playerOnTileRenderingLayerMask = 5, _objectLockRenderingLayerMask = 11;
    */
    private Renderer[] _renderers;
    private List<BoxCollider> _colliders = new();
    private Material _instancedLockedTileMat;
    private Material _instancedLockedObjectMat;

    public SelectionMode CurrentSelectionMode { get; private set; }

    class SelectionTweens
    {
        public TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> EnableSelectionTween;
        public TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> DisableSelectionTween;
        public TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> EnableGoldTween;
        public TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> DisableGoldTween;

        public SelectionTweens()
        {
            this.EnableSelectionTween = null;
            this.DisableSelectionTween = null;
            this.EnableGoldTween = null;
            this.DisableGoldTween = null;
        }
    }

    Dictionary<Renderer, SelectionTweens> _selectionCurrentValues = new();
    public bool IsTileLocked { get => _isTileLocked; set => _isTileLocked = value; }

    public enum SelectionMode
    {
        AXIS,
        CUBE,
        LOCKED,
        PLAYERONTILE,
        ENABLE,
        DISABLE,
        NOT_SELECTED
    }

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        foreach (var col in GetComponentsInChildren<BoxCollider>())
        {
            if (col.transform.CompareTag("ExteriorTileCollider"))
            {
                _colliders.Add(col);
            }
        }

        foreach (Renderer renderer in _renderers)
        {
            if (renderer.transform.CompareTag("Floor") || renderer.transform.CompareTag("SelectionShine"))
            {
                renderer.material = Instantiate(renderer.material);
                _selectionCurrentValues.Add(renderer, new SelectionTweens());
                _selectionCurrentValues[renderer] = new SelectionTweens();
            }
        }

        // disable all exterior colliders by default.
        ExteriorColiderEnabled(false);

        CurrentSelectionMode = SelectionMode.NOT_SELECTED;

        if (_isTileLocked)
        {
            Select(SelectionMode.LOCKED);
            foreach (var renderer in _renderers)
            {
                if (renderer.transform.CompareTag("Floor"))
                {
                    Material baseMat = renderer.sharedMaterial;
                    _instancedLockedTileMat = new Material(_lockedTileMat);

                    _instancedLockedTileMat.SetTexture("_BaseMap", baseMat.GetTexture("_Texture"));
                    _instancedLockedTileMat.SetTexture("_NormalMap", baseMat.GetTexture("_Normal"));
                    _instancedLockedTileMat.SetTexture("_MetallicRoughnessMap", baseMat.GetTexture("_MetallicRoughness"));
                    _instancedLockedTileMat.SetFloat("_RandomValue", UnityEngine.Random.Range(0.0f, 1.0f));

                    renderer.material = _instancedLockedTileMat;
                }
                else if (renderer.transform.CompareTag("LDObject"))
                {
                    Material baseMat = renderer.material;
                    _instancedLockedObjectMat = new Material(_lockedObjectMat);

                    _instancedLockedObjectMat.SetTexture("_BaseMap", baseMat.GetTexture("_BaseColorMap"));
                    _instancedLockedObjectMat.SetTexture("_NormalMap", baseMat.GetTexture("_NormalMap"));
                    _instancedLockedObjectMat.SetTexture("_MetallicRoughnessMap", baseMat.GetTexture("_MaskMap"));
                    _instancedLockedObjectMat.SetFloat("_RandomValue", UnityEngine.Random.Range(0.0f, 1.0f));

                    renderer.material = _instancedLockedObjectMat;
                }
            }
        }

        var vfx = GetComponentsInChildren<ParticleSystem>().Where(r => r.tag == "BizmuthFX").ToArray();
        foreach (var v in vfx)
        {
            if (v != null)
            {
                if (_isTileLocked)
                    v.Play();
                else
                    v.gameObject.SetActive(false); //Temp ? Idk why editor forces to play VFX sometimes
            }
        }
    }

    public void ExteriorColiderEnabled(bool isEnabled)
    {
        // when enabled, the collider will prevent the player from accessing the tile.
        foreach (BoxCollider collider in _colliders)
        {
            collider.enabled = isEnabled;
        }
    }

    public void Select(SelectionMode mode)
    {
        if (_renderers == null)
            return;
        if (mode == CurrentSelectionMode)
            return;
        foreach (var renderer in _renderers)
        {
            switch (mode)
            {
                case SelectionMode.AXIS:
                case SelectionMode.CUBE:
                    if (renderer.transform.CompareTag("Floor"))
                    {
                        renderer.material.SetFloat("_State", 0f);
                        _Select(renderer, GameManager.Instance.Settings.AxisSelectionFadeInDuration);
                    }
                    break;
                case SelectionMode.LOCKED:
                    if (renderer.transform.CompareTag("Floor"))
                    {
                        renderer.material.SetFloat("_State", 1f);
                        _Select(renderer, GameManager.Instance.Settings.AxisSelectionFadeInDuration);
                    }
                    break;
                case SelectionMode.PLAYERONTILE:
                    if (renderer.transform.CompareTag("Floor"))
                    {
                        renderer.material.SetFloat("_State", 2f);
                        _Select(renderer, GameManager.Instance.Settings.AxisSelectionFadeInDuration);
                    }
                    break;
                case SelectionMode.ENABLE:
                    renderer.enabled = true;
                    break;
                case SelectionMode.DISABLE:
                    renderer.enabled = false;
                    break;
            }
        }
        CurrentSelectionMode = mode;
    }

    public void Unselect()
    {
        if (CurrentSelectionMode == SelectionMode.NOT_SELECTED)
            return;
        foreach (var renderer in _renderers)
        {
            if ((CurrentSelectionMode == SelectionMode.AXIS || CurrentSelectionMode == SelectionMode.CUBE || CurrentSelectionMode == SelectionMode.LOCKED || CurrentSelectionMode == SelectionMode.PLAYERONTILE)
                && (renderer.transform.CompareTag("Floor")))
            {
                _Unselect(renderer, GameManager.Instance.Settings.AxisSelectionFadeOutDuration);
            }
            /*
            else
            {
                renderer.renderingLayerMask = (uint)Mathf.Pow(2, _defaultRenderingLayerMask);
            }
            */
        }
        CurrentSelectionMode = SelectionMode.NOT_SELECTED;
    }

    public void StartShineAnim()
    {
        //if (CurrentSelectionMode == SelectionMode.AXIS || CurrentSelectionMode == SelectionMode.CUBE)
        StartCoroutine(ShineAnim());
    }
    private IEnumerator ShineAnim()
    {
        foreach (var renderer in _renderers)
        {
            if (renderer.transform.CompareTag("SelectionShine"))
            {
                _Select(renderer, 0.8f, Ease.OutQuint);
            }
            else if (renderer.transform.CompareTag("Floor"))
            {
                _Unselect(renderer, GameManager.Instance.Settings.AxisSelectionFadeOutDuration);
            }
        }
        yield return new WaitForSeconds(GameManager.Instance.Settings.RubikscCubeAxisRotationDuration);
        foreach (var renderer in _renderers)
        {
            if (renderer.transform.CompareTag("SelectionShine"))
            {
                _Unselect(renderer, 0.8f, Ease.InQuint);
            }
        }
    }

    public void StartActivateExteriorColliders()
    {
        StartCoroutine(ActivateExteriorColliders());
    }
    private IEnumerator ActivateExteriorColliders()
    {
        ExteriorColiderEnabled(true);
        yield return new WaitForSeconds(GameManager.Instance.Settings.RubikscCubeAxisRotationDuration);
        ExteriorColiderEnabled(false);
    }

    public void StartCorrectActionAnim()
    {
        StartCoroutine(CorrectActionAnim());
    }

    private IEnumerator CorrectActionAnim()
    {
        foreach (var renderer in _renderers)
        {
            if (renderer.transform.CompareTag("SelectionShine"))
            {
                _Select(renderer, GameManager.Instance.Settings.AxisSelectionFadeOutDuration);

                if (_selectionCurrentValues.ContainsKey(renderer))
                {
                    if (_selectionCurrentValues[renderer].EnableGoldTween != null && _selectionCurrentValues[renderer].EnableGoldTween.active)
                        break;

                    if (_selectionCurrentValues[renderer].DisableGoldTween != null && _selectionCurrentValues[renderer].DisableGoldTween.active)
                        _selectionCurrentValues[renderer].DisableGoldTween.Kill();

                    _selectionCurrentValues[renderer].EnableGoldTween = DOTween.To(() => renderer.material.GetFloat("_Gold_Slider"), x => renderer.material.SetFloat("_Gold_Slider", x), 1.0f, 0.1f).SetEase(Ease.InOutSine);
                }
            }
        }
        yield return new WaitForSeconds(0.1f);
        foreach (var renderer in _renderers)
        {
            if (renderer.transform.CompareTag("SelectionShine"))
            {
                _Unselect(renderer, GameManager.Instance.Settings.AxisSelectionFadeOutDuration);

                if (_selectionCurrentValues.ContainsKey(renderer))
                {
                    if (_selectionCurrentValues[renderer].DisableGoldTween != null && _selectionCurrentValues[renderer].DisableGoldTween.active)
                        break;
                    if (_selectionCurrentValues[renderer].EnableGoldTween != null && _selectionCurrentValues[renderer].EnableGoldTween.active)
                        _selectionCurrentValues[renderer].EnableGoldTween.Kill();

                    _selectionCurrentValues[renderer].DisableGoldTween = DOTween.To(() => renderer.material.GetFloat("_Gold_Slider"), x => renderer.material.SetFloat("_Gold_Slider", x), 0.0f, 0.5f).SetEase(Ease.InOutSine);
                }
            }
        }

    }

    private void _Select(Renderer renderer, float duration, Ease ease = Ease.InOutSine)
    {
        if (_selectionCurrentValues.ContainsKey(renderer))
        {
            if (_selectionCurrentValues[renderer].EnableSelectionTween != null && _selectionCurrentValues[renderer].EnableSelectionTween.active)
                return;

            if (_selectionCurrentValues[renderer].DisableSelectionTween != null && _selectionCurrentValues[renderer].DisableSelectionTween.active)
                _selectionCurrentValues[renderer].DisableSelectionTween.Kill();

            _selectionCurrentValues[renderer].EnableSelectionTween = DOTween.To(() => renderer.material.GetFloat("_Alpha_shader"), x => renderer.material.SetFloat("_Alpha_shader", x), 1.0f, duration).SetEase(ease);
        }
    }

    private void _Unselect(Renderer renderer, float duration, Ease ease = Ease.InOutSine)
    {
        if (_selectionCurrentValues.ContainsKey(renderer))
        {
            if (_selectionCurrentValues[renderer].DisableSelectionTween != null && _selectionCurrentValues[renderer].DisableSelectionTween.active)
                return;
            if (_selectionCurrentValues[renderer].EnableSelectionTween != null && _selectionCurrentValues[renderer].EnableSelectionTween.active)
                _selectionCurrentValues[renderer].EnableSelectionTween.Kill();

            _selectionCurrentValues[renderer].DisableSelectionTween = DOTween.To(() => renderer.material.GetFloat("_Alpha_shader"), x => renderer.material.SetFloat("_Alpha_shader", x), 0.0f, duration).SetEase(ease);
        }
    }

    public void BizmuthShineAnim()
    {
        if (!_isTileLocked) return;

        foreach (Renderer renderer in _renderers)
        {
            if (renderer.transform.CompareTag("Floor"))
            {
                DOTween.To(() => 0.0f, x => renderer.material.SetFloat("_AnimDelta", x), 1.0f, 1.0f).SetEase(_bizmuthShineCurve);
            }
        }
    }
}

