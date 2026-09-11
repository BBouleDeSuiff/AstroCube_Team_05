using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using NaughtyAttributes;

public class MemoryObject : MonoBehaviour, IInteractable
{
    //[SerializeField] private MemoryCharacter _memoryCharacterPrefab;
    //[SerializeField] private List<Vector3> _characterPositions = new();
    [SerializeField] private List<MemoryVFXController> _memories = new();
    [SerializeField] private List<GameObject> _gameObjectsToActivate = new();
    [SerializeField] private List<SubtitleData> _subtitles = new();
    [SerializeField] private Material _memoryMat;

    [Header("FMOD")]
    [SerializeField] private EventReference _startMemoryEvent;
    [SerializeField] private EventReference _stopMemoryEvent;
    [SerializeField] private float _delayBeforeStopEvent = 3f;
    
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private MeshRenderer _teapotRenderer;
    [SerializeField] private GameObject _outlineObject;
    public UnityEvent OnMemoryInteracted, OnCharacterAnimationFinished, OnAnimationFinished;
    
    //private List<MemoryCharacter> _characters = new();
    private bool _isSkipped = false;
    public bool IsSkipped => _isSkipped;
    
    private bool _wasPlayed;
    private bool _skipState;
    private float _skipDelay;

    private void OnValidate()
    {
        if (_gameObjectsToActivate == null || _gameObjectsToActivate.Count == 0)
            return;
        
        if(!_teapotRenderer)
            return;
        
        foreach (GameObject obj in _gameObjectsToActivate)
        {
            if (obj.TryGetComponent(out MeshRenderer mesh))
            {
                mesh.material = new Material( _memoryMat);
                _teapotRenderer.materials[1] = new Material(_teapotRenderer.materials[1]);
                _teapotRenderer.materials[1].SetFloat("_Alpha", 1.0f);
                mesh.enabled = true;
            }
        }
    }

    private IEnumerator StartMemory()
    {
        _wasPlayed = true;

        EventManager.OnSkipCutscene += SetSkipCutsceneState;
        
        if (!_startMemoryEvent.IsNull) RuntimeManager.PlayOneShot(_startMemoryEvent, transform.position);

        DOTween.To(() => _teapotRenderer.materials[1].GetFloat("_Alpha"),
            (x) => _teapotRenderer.materials[1].SetFloat("_Alpha", x), 1.2f, 0.5f).SetEase(Ease.InOutExpo);
        DOTween.To(() => _teapotRenderer.materials[1].GetFloat("_FresnelPower"),
            (x) => _teapotRenderer.materials[1].SetFloat("_FresnelPower", x), 3.0f, 0.5f).SetEase(Ease.InOutExpo);
        
        for (var index = 0; index < _memories.Count; index++)
        {
            var mem = _memories[index];
            try
            {
                mem.StartVFX(_gameObjectsToActivate[index]);
            }
            catch
            {
                mem.StartVFX(null);
            }
        }
        
        foreach (SubtitleData subtitle in _subtitles)
        {
            if (_isSkipped)
                continue;
            
            if (subtitle.isEmpty == false)
            {
                if (_memories.Count > subtitle.characterIndex && _memories[subtitle.characterIndex] != null)
                {
                    var characterVoice = _memories[subtitle.characterIndex].GetComponent<AUDIO_CharacterVoice>();
                    if (characterVoice != null && !string.IsNullOrEmpty(subtitle.audioKey))
                    {
                        characterVoice.PlayVoice(subtitle.audioKey);
                    }
                    
                    LocalizationManager.Instance.PrintStringFromID(subtitle.csvName, subtitle.localizationID, subtitle.locutor, subtitle.color);
                    yield return new WaitUntil(() => IsSkipped, new TimeSpan(0, 0, 0, (int) subtitle.duration, (int)(subtitle.duration % 1.0f)), () => {}, WaitTimeoutMode.InGameTime);
                    characterVoice.Cancel();
                }
            }
            else
            {
                yield return new WaitUntil(() => IsSkipped, new TimeSpan(0, 0, 0, (int) subtitle.duration, (int)(subtitle.duration % 1.0f)), () => {}, WaitTimeoutMode.InGameTime);   
            }
        }
        LocalizationManager.Instance.ClearString();
        
        yield return new WaitForSeconds(_delayBeforeStopEvent);

        DOTween.To(() => _teapotRenderer.materials[1].GetFloat("_Alpha"),
            (x) => _teapotRenderer.materials[1].SetFloat("_Alpha", x), 0f, 1f).SetEase(Ease.InOutExpo);
        _particleSystem.Stop();
        
        EventManager.OnSkipCutscene -= SetSkipCutsceneState;
        
        if (!_stopMemoryEvent.IsNull) RuntimeManager.PlayOneShot(_stopMemoryEvent, transform.position);
    }

    private void Update()
    {
        if (_isSkipped)
            return;
        
        if (_skipState)
            _skipDelay += Time.deltaTime;
        else
            _skipDelay = 0.0f;
        LocalizationManager.Instance.SetSkipValue(_skipDelay / GameManager.Instance.Settings.SkipCutsceneDuration);
        
        if(_skipDelay >= GameManager.Instance.Settings.SkipCutsceneDuration)
            SkipCutscene();
    }

    private void SetSkipCutsceneState(bool state)
    {
        _skipState = state;
    }

    private void SkipCutscene() => _isSkipped = true;

    public void OnInteract()
    {
        if (!_wasPlayed)
        {
            gameObject.layer = LayerMask.NameToLayer("MemoryObject");
            
            StartCoroutine(StartMemory());
            Destroy(_outlineObject);
            OnMemoryInteracted?.Invoke();
        }
    }

    public void SetOutline(bool state)
    {
        _outlineObject.SetActive(state);
    }

    public void GrantAchievement(string id)
    {
        AchievementManager.Instance.UnlockAchievement(id);
    }
}

[Serializable]
public struct SubtitleData
{
    public bool isEmpty;
    private bool MustShowParameter => !isEmpty;
    
    [ShowIf("MustShowParameter")] public string locutor;
    [ShowIf("MustShowParameter")] public string csvName;
    [ShowIf("MustShowParameter")] public string localizationID;
    public float duration;
    [ShowIf("MustShowParameter")] public Color color;
    [ShowIf("MustShowParameter")] public string audioKey;
    [ShowIf("MustShowParameter")] public int characterIndex;
}
