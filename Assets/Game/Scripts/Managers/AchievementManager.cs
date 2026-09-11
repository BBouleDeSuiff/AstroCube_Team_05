using System;
using System.Collections;
using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }
    
    private float _currentTime;
    private int _currentAmountOfRotations;
    private int _lockedRotations;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
#if !DISABLESTEAMWORKS
        _currentTime = PlayerPrefs.HasKey("CURRENT_TIME") ? PlayerPrefs.GetFloat("CURRENT_TIME") : 0f;
        _currentAmountOfRotations = PlayerPrefs.HasKey("ROTATION_AMOUNT") ? PlayerPrefs.GetInt("ROTATION_AMOUNT") : 0;
        _lockedRotations = PlayerPrefs.HasKey("LOCKED_ROTATIONS") ? PlayerPrefs.GetInt("LOCKED_ROTATIONS") : 0;
        
        StartCoroutine(SaveCoroutine());
#endif
    }

    public void UnlockAchievement(string key)
    {
#if !DISABLESTEAMWORKS
        if (!SteamManager.Initialized)
            return;

        SteamUserStats.GetAchievement(key, out bool achieved);
        if (!achieved)
        {
            SteamUserStats.SetAchievement(key);
            SteamUserStats.StoreStats();
        }
#endif
    }

    private void Update()
    {
#if !DISABLESTEAMWORKS
        _currentTime += Time.deltaTime;
#endif
    }

    private IEnumerator SaveCoroutine()
    {
#if !DISABLESTEAMWORKS
        while (gameObject.activeSelf)
        {
            PlayerPrefs.SetFloat("CURRENT_TIME", _currentTime);
            PlayerPrefs.SetInt("ROTATION_AMOUNT", _currentAmountOfRotations);
            PlayerPrefs.SetInt("LOCKED_ROTATIONS", _lockedRotations);
            PlayerPrefs.Save();
            
            yield return new WaitForSecondsRealtime(1f);
        }
#endif
#if DISABLESTEAMWORKS
        yield return false;

#endif
    }

    public void EndGame()
    {
#if !DISABLESTEAMWORKS
        if(_currentTime <= GameManager.Instance.Settings.MaxTimeForAchievement)
        {
            UnlockAchievement("SPEEDRUNNER");
        }
    
        if(_currentAmountOfRotations <= GameManager.Instance.Settings.MaxRotationsForAchievement)
        {
            UnlockAchievement("EFFICIENCY");
        }
#endif
    }

    public void AddRotation(bool locked = false)
    {
#if !DISABLESTEAMWORKS
        if (locked)
        {
            _lockedRotations++;
            if (_lockedRotations >= GameManager.Instance.Settings.MaxLockedRotationsForAchievement)
            {
                UnlockAchievement("DOOR_STUCK");
            }
        }
        else
        {
            _currentAmountOfRotations++;
        }
#endif
    }
}
