using System;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using NaughtyAttributes;

namespace Unity.Multiplayer.Widgets
{
    public class SessionUiManager : MonoBehaviour
    {
        [SerializeField] private Button _startSessionButton;

        [SerializeField] private TMP_Text _sessionCodeText;

        [SerializeField] private Button _copySessionCodeButton;

        [SerializeField] private TMP_InputField _joinSessionInputField;

        [SerializeField] private Button _joinSessionButton;

        [SerializeField] private Button _leaveSessionButton;

        [SerializeField] private VerticalLayoutGroup _playerListGroup;

        [SerializeField] private SessionPlayerListItem _playerListItemPrefab;

        private ISession _currentSession;

        [SerializeField] private Button _changeToSteerButton;

        [SerializeField] private Button _changeToPedalButton;

        [SerializeField] private Button _refreshPlayerListButton;

        [SerializeField] private Button _startGameButton;

        private List<SessionPlayerListItem> _playerListItems = new();

        private void OnEnable()
        {
            _startSessionButton.onClick.AddListener(OnStartSessionClicked);
            _joinSessionButton.onClick.AddListener(OnJoinSessionClicked);
            _leaveSessionButton.onClick.AddListener(OnLeaveSessionClicked);
            _joinSessionInputField.onValueChanged.AddListener(OnJoinSessionInputChanged);
            _sessionCodeText.text = SessionManager.Instance.ActiveSession?.Code ?? "";
            _copySessionCodeButton.onClick.AddListener(() => { GUIUtility.systemCopyBuffer = _sessionCodeText.text; });
            SessionManager.Instance.OnSessionChanged -= OnSessionChanged;
            SessionManager.Instance.OnSessionChanged += OnSessionChanged;
            _changeToPedalButton.onClick.AddListener(() =>
            {
                SessionManager.Instance.SetPlayerRole(PlayerRole.Pedal);
            });
            _changeToSteerButton.onClick.AddListener(() =>
            {
                SessionManager.Instance.SetPlayerRole(PlayerRole.Steer);
            });
            _refreshPlayerListButton.onClick.AddListener(UpdatePlayersList);
            _startGameButton.onClick.AddListener(() => { SessionManager.Instance.StartGame(); });
        }

        private void OnDisable()
        {
            _startSessionButton.onClick.RemoveListener(OnStartSessionClicked);
            _joinSessionButton.onClick.RemoveListener(OnJoinSessionClicked);
            _leaveSessionButton.onClick.RemoveListener(OnLeaveSessionClicked);
            _joinSessionInputField.onValueChanged.RemoveListener(OnJoinSessionInputChanged);
            _copySessionCodeButton.onClick.RemoveAllListeners();
            _changeToPedalButton.onClick.RemoveAllListeners();
            _changeToSteerButton.onClick.RemoveAllListeners();
            _refreshPlayerListButton.onClick.RemoveAllListeners();
            _startGameButton.onClick.RemoveAllListeners();
            _sessionCodeText.text = "";
            SessionManager.Instance.OnSessionChanged -= OnSessionChanged;
        }

        private void OnSessionChanged(ISession session)
        {
            Debug.Log("Session changed, updating UI...");
            // Update the session code text when the session changes
            _sessionCodeText.text = session?.Code ?? "";

            // Enable or disable the leave button based on whether there is an active session
            _leaveSessionButton.interactable = session != null;

            // Reset the join input field and button state
            _joinSessionInputField.text = "";
            _joinSessionButton.interactable = false;
            _startSessionButton.interactable = false;

            // Unregister from the previous session's events
            UnRegisterFromSessionEvents();

            _currentSession = session;
            // Register to the new session's events
            RegisterToSessionEvents();

            // Update the player list
            UpdatePlayersList();
        }

        private void RegisterToSessionEvents()
        {
            if (_currentSession == null)
                return;
            Debug.Log("Registering to session events...");
            _currentSession.PlayerJoined += OnPlayerJoined;
            _currentSession.PlayerHasLeft += OnPlayerLeft;
            _currentSession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
        }

        private void UnRegisterFromSessionEvents()
        {
            if (_currentSession == null)
                return;
            Debug.Log("Unregistering from session events...");
            _currentSession.PlayerJoined -= OnPlayerJoined;
            _currentSession.PlayerHasLeft -= OnPlayerLeft;
            _currentSession.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;
        }

        private void OnPlayerPropertiesChanged()
        {
            Debug.Log("Player properties changed, updating player roles...");
            ChangePlayerRole();
        }

        private void ChangePlayerRole()
        {
            if (_currentSession == null || _currentSession.CurrentPlayer == null)
                return;

            // Get the current player's role from their properties
            if (_currentSession.CurrentPlayer.Properties.TryGetValue(MultiplayerKeys.PlayerRole,
                    out var playerRoleProperty))
            {
                if (Enum.TryParse(playerRoleProperty.Value, out PlayerRole role))
                {
                    // Set the button states based on the player's role
                    _changeToSteerButton.interactable = role != PlayerRole.Steer;
                    _changeToPedalButton.interactable = role != PlayerRole.Pedal;
                }
                else
                {
                    Debug.LogWarning("Invalid player role: " + playerRoleProperty.Value);
                }
            }

            foreach (var player in _currentSession.Players)
            {
                _playerListItems.Find(x => x.PlayerId == player.Id)?.SetRole(
                    player.Properties.TryGetValue(MultiplayerKeys.PlayerRole, out var roleProperty) &&
                    Enum.TryParse(roleProperty.Value, out PlayerRole role)
                        ? role
                        : PlayerRole.None);
            }
        }

        private void OnPlayerLeft(string playerId)
        {
            UpdatePlayersList();
        }

        private void OnPlayerJoined(string playerId)
        {
            UpdatePlayersList();
        }

        [Button]
        public void UpdatePlayersList()
        {
            Debug.Log("Updating player list...");
            // Clear the current player list
            foreach (Transform child in _playerListGroup.transform)
            {
                Destroy(child.gameObject);
            }

            _playerListItems.Clear();

            // If there is no active session, return
            if (_currentSession == null)
                return;

            // Create a new player list item for each player in the session
            foreach (var player in _currentSession.Players)
            {
                var playerListItem = Instantiate(_playerListItemPrefab, _playerListGroup.transform);
                var playerName = SessionManager.Instance.ActiveSession.Players.FirstOrDefault(p => p.Id == player.Id)
                    ?.Properties.TryGetValue(MultiplayerKeys.PlayerName, out var playerNameProperty) == true
                    ? playerNameProperty.Value
                    : "Unknown";
                var playerRole = SessionManager.Instance.ActiveSession.Players.FirstOrDefault(p => p.Id == player.Id)
                    ?.Properties.TryGetValue(MultiplayerKeys.PlayerRole, out var playerRoleProperty) == true
                    ? playerRoleProperty.Value
                    : "None";
                playerListItem.Initialize(playerName, player.Id);
                if (Enum.TryParse(playerRole, out PlayerRole role))
                    playerListItem.SetRole(role);
                else
                    playerListItem.SetRole(PlayerRole.None);

                _playerListItems.Add(playerListItem);
            }
        }

        private void OnJoinSessionInputChanged(string arg0)
        {
            // Enable or disable the join button based on whether the input field is empty
            _joinSessionButton.interactable = !string.IsNullOrEmpty(arg0);
        }

        private void OnStartSessionClicked()
        {
            SessionManager.Instance.StartSessionAsHost();
        }

        private void OnJoinSessionClicked()
        {
            var sessionCode = _joinSessionInputField.text;
            if (!string.IsNullOrEmpty(sessionCode))
            {
                SessionManager.Instance.JoinSessionByCode(sessionCode);
            }
            else
            {
                Debug.LogWarning("Oturum kodu girilmedi.");
            }
        }

        private void OnLeaveSessionClicked()
        {
            SessionManager.Instance.LeaveSession();
        }
    }
}