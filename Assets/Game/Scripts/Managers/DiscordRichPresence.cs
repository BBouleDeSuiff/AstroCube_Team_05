using System;
using DiscordRPC;
using DiscordRPC.Logging;
using Lachee.Discord.Control;
using UnityEngine;
using ILogger = UnityEngine.ILogger;

public class DiscordRichPresence : MonoBehaviour
{
    private const string ApplicationID = "1451332609194987540";
    private DiscordRpcClient _client;
    
    [SerializeField] private string details;
    [SerializeField] private string state;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _client = new DiscordRpcClient(ApplicationID);
        
        _client.Logger = new ConsoleLogger() { Level = LogLevel.Trace };
        _client.Logger = new ConsoleLogger() { Level = LogLevel.Trace };
        
        _client.OnReady += (sender, e) =>
        {
            Debug.Log($"Discord connecté ! User: {e.User.Username}");
        };
        
        _client.OnPresenceUpdate += (sender, e) =>
        {
            Debug.Log("Rich Presence mise à jour !");
        };
        
        _client.OnError += (sender, e) =>
        {
            Debug.LogError($"Erreur Discord: {e.Message}");
        };
        
        _client.OnConnectionFailed += (sender, e) =>
        {
            Debug.LogError($"Connexion échouée: {e.Type}");
        };
        
        _client.Initialize();
        Debug.Log("Client initialisé");

        UpdateRichPresence();
    }
    
    private void Update()
    {
        if (_client != null)
        {
            _client.Invoke();
        }
    }

    public void UpdateRichPresence()
    {
        if (_client == null || !_client.IsInitialized) return;

        _client.SetPresence(new RichPresence()
        {
            Details = details,
#if UNITY_EDITOR
            State = "Resolving the code of the cube",
#else
            State = state,
#endif
            Assets = new Assets()
            {
                LargeImageKey = "gamelogo",
                SmallImageKey = "gamelogo",
            },
            Timestamps = new Timestamps()
            {
                Start = System.DateTime.UtcNow
            }
        });
    }
    
    private void OnApplicationQuit()
    {
        if (_client != null)
        {
            _client.Dispose();
        }
    }

    private void OnDestroy()
    {
        if (_client != null)
        {
            _client.Dispose();
        }
    }
}
