using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
using UnityEngine.Scripting;

#if !DISABLESTEAMWORKS
[Preserve]
public class SteamOverlayHandler : MonoBehaviour
{

    private Callback<GameOverlayActivated_t> _gameOverlayActivated;

    private void Start()
    {

        if (SteamManager.Initialized)
        {
            _gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        }

    }

    private void OnGameOverlayActivated(GameOverlayActivated_t callback)
    {
        bool overlayActive = callback.m_bActive == 1;
        if (overlayActive)
        {
            EventManager.TriggerGamePause();
        }
    }
}
#endif