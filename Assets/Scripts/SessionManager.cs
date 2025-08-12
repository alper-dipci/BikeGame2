using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace;
using NaughtyAttributes;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SessionManager : SingletonMonoBehaviour<SessionManager>
{
    private ISession _activeSession;
    public bool ServicesInitialized => UnityServices.State == ServicesInitializationState.Initialized;
    public event Action<ISession> OnSessionChanged;
    public event Action<PlayerRole> OnLocalPlayerRoleChanged;

    public PlayerRole MyRole
    {
        get
        {
            if (ActiveSession == null || ActiveSession.CurrentPlayer == null)
            {
                return PlayerRole.None; 
            }

            if (ActiveSession.CurrentPlayer.Properties.TryGetValue(MultiplayerKeys.PlayerRole, out var roleProp))
            {
                return Enum.TryParse(roleProp.Value, out PlayerRole role) ? role : PlayerRole.None;
            }

            return PlayerRole.None;
        }
    }

    public ISession ActiveSession
    {
        get => _activeSession;
        set
        {
            if (_activeSession == value)
                return; // aynı session ise hiçbir şey yapma
            Debug.Log($"Active session changed from {_activeSession?.Id} to {value?.Id}");
            if (_activeSession != null)
            {
                UnregisterSessionEvents();
                _activeSession = null; // Clear before assigning new value
            }

            // Then assign and subscribe to new session
            _activeSession = value;

            if (_activeSession != null)
            {
                RegisterSessionEvents();
            }

            OnSessionChanged.Invoke(value);
        }
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"Unity Services initialized and signed in Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [Button]
    public async void StartSessionAsHost()
    {
        //TODO: kendime not burda key value degerleri atanıp birşeyler eklenebilir.  PlayerProperty ve SessionProperty var 
        //
        // var  sessionProperties = new Dictionary<string, SessionProperty>
        // {
        //     { "Map", new SessionProperty("Forest", VisibilityPropertyOptions.Public) },
        //     { "GameMode", new SessionProperty("Capture the Flag", VisibilityPropertyOptions.Public) }
        // };

        var playerProperties = GetPlayerProperties();
        var options = new SessionOptions()
        {
            MaxPlayers = 3,
            IsPrivate = false,
            IsLocked = false,
            PlayerProperties = playerProperties
        }.WithRelayNetwork();

        var session = await MultiplayerService.Instance.CreateSessionAsync(options);
        if (session != null)
        {
            ActiveSession = session;
            Debug.Log($"Session created with ID: {ActiveSession.Id}, Join Code: {ActiveSession.Code}");
        }
        else
        {
            Debug.LogError("Session creation returned null.");
        }
    }

    private Dictionary<string, PlayerProperty> GetPlayerProperties()
    {
        var playerName = AuthenticationService.Instance.PlayerName;
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Public);
        var playerRoleProperty = new PlayerProperty("Player", VisibilityPropertyOptions.Public);
        return new Dictionary<string, PlayerProperty>()
        {
            { MultiplayerKeys.PlayerName, playerNameProperty },
            { MultiplayerKeys.PlayerRole, playerRoleProperty }
        };
    }

    public async void JoinSessionById(string sessionId)
    {
        try
        {
            ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
            Debug.Log($"Joined session with ID: {ActiveSession.Id}, Join Code: {ActiveSession.Code}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join session: {e.Message}");
        }
    }

    public async void JoinSessionByCode(string joinCode)
    {
        try
        {
            ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);
            Debug.Log($"Joined session with Join Code: {ActiveSession.Code}, ID: {ActiveSession.Id}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Button]
    public async void LeaveSession()
    {
        if (ActiveSession == null)
        {
            Debug.LogWarning("No active session to leave.");
            return;
        }

        try
        {
            await ActiveSession.LeaveAsync();
        }
        catch (Exception e)
        {
            // ignore beacuse we leavin
            Debug.LogWarning("Failed to leave session: " + e.Message);
        }
        finally
        {
            ActiveSession = null;
            Debug.Log("Left the session successfully.");
        }
    }

    [Button]
    public void SetPlayerRoleToSteer()
    {
        SetPlayerRole(PlayerRole.Steer);
    }

    [Button]
    public void SetPlayerRoleToPedal()
    {
        SetPlayerRole(PlayerRole.Pedal);
    }

    public async void SetPlayerRole(PlayerRole newRole)
    {
        if (ActiveSession == null || ActiveSession.CurrentPlayer == null)
        {
            Debug.LogWarning("No active session or current player to change role.");
            return;
        }

        try
        {
            ActiveSession.CurrentPlayer.SetProperty(MultiplayerKeys.PlayerRole, new PlayerProperty(newRole.ToString()));
            await ActiveSession.SaveCurrentPlayerDataAsync();
            OnLocalPlayerRoleChanged?.Invoke(newRole);
            Debug.Log($"Player role changed to: {newRole}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to change player role: {e.Message}");
        }
    }

    [Button]
    public void StartGame()
    {
        if (!CanStartTheGame())
            return;
        CustomSceneManager.Instance.LoadNetworkScene(SceneKeys.GameScene);
    }

    private bool CanStartTheGame()
    {
        if (ActiveSession == null)
        {
            Debug.LogWarning("No active session to start the game.");
            return false;
        }

        if (!ActiveSession.IsHost)
        {
            Debug.LogWarning("Only the host can start the game.");
            return false;
        }

        // if (ActiveSession.Players.Count != 2)
        // {
        //     Debug.LogWarning("Not enough players to start the game. At least 2 players are required.");
        //     return false;
        // }
        // bool hasSteer = ActiveSession.Players.Any(p =>
        //     p.Properties.TryGetValue(MultiplayerKeys.PlayerRole, out var roleProp) &&
        //     roleProp.Value == PlayerRole.Steer.ToString());
        //
        // bool hasPedal = ActiveSession.Players.Any(p =>
        //     p.Properties.TryGetValue(MultiplayerKeys.PlayerRole, out var roleProp) &&
        //     roleProp.Value == PlayerRole.Pedal.ToString());
        //
        // if (!hasSteer || !hasPedal)
        // {
        //     Debug.LogWarning("Both Steer and Pedal roles are required to start the game.");
        //     return false;
        // }

        return true;
    }

    #region Events

    private void RegisterSessionEvents()
    {
        if (ActiveSession == null) return;
        Debug.Log("Registering session events for: " + ActiveSession.Id);

        ActiveSession.Changed += ActiveSessionChanged;
        ActiveSession.StateChanged += ActiveSessionStateChanged;
        ActiveSession.PlayerJoined += ActiveSessionPlayerJoined;
        ActiveSession.PlayerLeaving += ActiveSessionPlayerLeaving;
        ActiveSession.PlayerHasLeft += ActiveSessionPlayerHasLeft;
        ActiveSession.SessionPropertiesChanged += ActiveSessionSessionPropertiesChanged;
        ActiveSession.PlayerPropertiesChanged += ActiveSessionPlayerPropertiesChanged;
        ActiveSession.RemovedFromSession += ActiveSessionRemovedFromSession;
        ActiveSession.Deleted += ActiveSessionDeleted;
        ActiveSession.SessionHostChanged += ActiveSessionSessionHostChanged;
    }

    private void UnregisterSessionEvents()
    {
        if (ActiveSession == null) return;
        Debug.Log("UnRegistering session events for: " + ActiveSession.Id);
        ActiveSession.Changed -= ActiveSessionChanged;
        ActiveSession.StateChanged -= ActiveSessionStateChanged;
        ActiveSession.PlayerJoined -= ActiveSessionPlayerJoined;
        ActiveSession.PlayerLeaving -= ActiveSessionPlayerLeaving;
        ActiveSession.PlayerHasLeft -= ActiveSessionPlayerHasLeft;
        ActiveSession.SessionPropertiesChanged -= ActiveSessionSessionPropertiesChanged;
        ActiveSession.PlayerPropertiesChanged -= ActiveSessionPlayerPropertiesChanged;
        ActiveSession.RemovedFromSession -= ActiveSessionRemovedFromSession;
        ActiveSession.Deleted -= ActiveSessionDeleted;
        ActiveSession.SessionHostChanged -= ActiveSessionSessionHostChanged;
    }

    // Event fonksiyonları

    private void ActiveSessionChanged()
    {
        Debug.Log("Active session changed.");
    }

    private void ActiveSessionStateChanged(SessionState state)
    {
        Debug.Log($"Active session state changed to: {state}");
    }

    private void ActiveSessionPlayerJoined(string playerId)
    {
        Debug.Log($"Oyuncu katıldı: {playerId}");
    }

    private void ActiveSessionPlayerLeft(string playerId)
    {
        Debug.Log($"Oyuncu ayrıldı: {playerId}");
    }

    private void ActiveSessionPlayerLeaving(string playerId)
    {
        Debug.Log($"Oyuncu ayrılıyor: {playerId}");
    }

    private void ActiveSessionPlayerHasLeft(string playerId)
    {
        Debug.Log($"Oyuncu tamamen ayrıldı: {playerId}");
    }

    private void ActiveSessionSessionPropertiesChanged()
    {
        Debug.Log("Oturum özellikleri değişti.");
    }

    private void ActiveSessionPlayerPropertiesChanged()
    {
        Debug.Log($"ActiveSessionPlayerPropertiesChanged:");
    }

    private void ActiveSessionRemovedFromSession()
    {
        Debug.Log("Oturumdan çıkarıldın.");
    }

    private void ActiveSessionDeleted()
    {
        Debug.Log("Oturum silindi.");
    }

    private void ActiveSessionSessionHostChanged(string newHostPlayerId)
    {
        Debug.Log($"Oturumun yeni host'u: {newHostPlayerId}");
    }

    #endregion
}