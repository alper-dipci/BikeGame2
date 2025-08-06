using System;
using DefaultNamespace;
using NaughtyAttributes;
using rayzngames;
using UnityEngine;
using Unity.Netcode;

public class BicycleInputReader : NetworkBehaviour
{
    private BicycleVehicle _bicycleVehicle;

    private PlayerRole _myRole;

    private bool _shouldListenToInput = false;

    public override void OnNetworkSpawn()
    {
        Debug.Log("BicycleInputReader enabled. + isOwner: " + IsOwner);
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        SceneManager.Instance.OnSceneLoaded += OnSceneLoaded;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("BicycleInputReader disabled.");
        base.OnNetworkDespawn();
    }

    private void OnSceneLoaded(string sceneName)
    {
        if(sceneName != SceneKeys.GameScene)
        {
            _shouldListenToInput = false;
            return;
        }
        _bicycleVehicle = FindFirstObjectByType<BicycleVehicle>();
        Debug.Log("BicycleInputReader is enabled and listening to input.");
        
        _myRole = GetMyRoleFromSession();
        Debug.Log($"My role is: {_myRole}");
        
        _shouldListenToInput = true;
    }

    [Button]
    public void DebugSTuff()
    {
        if(!_shouldListenToInput)
            Debug.Log("BicycleInputReader is not listening to input because the scene is not the game scene.");
        if (!_bicycleVehicle)
            Debug.Log("BicycleVehicle is not found in the scene. Make sure it is present before enabling input reading.");
        if(!IsOwner)
            Debug.Log("BicycleInputReader is not the owner. Input reading is disabled for this instance.");
        
    }

    private void Update()
    {
        
        if (!IsOwner || !_shouldListenToInput || !_bicycleVehicle) return;

        float horizontal = 0f;
        float vertical = 0f;
        bool braking = false;

        switch (_myRole)
        {
            case PlayerRole.Steer:
                horizontal = Input.GetAxis("Horizontal");
                _bicycleVehicle.SetHorizontalInputRpc(horizontal);
                break;
            case PlayerRole.Pedal:
                vertical = Input.GetAxis("Vertical");
                braking = Input.GetKey(KeyCode.Space);
                _bicycleVehicle.SetVerticalInputRpc(vertical);
                _bicycleVehicle.SetBrakingRpc(braking);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private PlayerRole GetMyRoleFromSession()
    {
        // Multiplayer Session’dan kendi rolünü çek
        var player = SessionManager.Instance.ActiveSession?.CurrentPlayer;
        if (player != null && player.Properties.TryGetValue(MultiplayerKeys.PlayerRole, out var prop))
        {
            if (System.Enum.TryParse(prop.Value.ToString(), out PlayerRole parsedRole))
                return parsedRole;
        }
        Debug.LogError("Player role not found in session properties.");
        return PlayerRole.Pedal; // varsayılan
    }
}