using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;

public class VehicleInputController : NetworkBehaviour
{
    [SerializeField] private Player Player;

    private float _lastHorizontal;
    private bool _lastPedaling;
    private bool _lastBraking;

    private NetworkVehicleBase _currentVehicle;

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnVehicleChanged += OnVehicleChanged;
        base.OnNetworkSpawn();
    }

    private void OnVehicleChanged()
    {
        _currentVehicle = GameManager.Instance.CurrentVehicle;
        //_vehicle.SetInputReader(this);
    }

    private void Update()
    {
        if (!IsOwner || _currentVehicle == null) return;

        switch (Player.PlayerRole)
        {
            case PlayerRole.Steer:
                float horizontal = LocalInputReader.Instance.HorizontalInput;
                if (Mathf.Abs(horizontal - _lastHorizontal) > 0.01f)
                {
                    _lastHorizontal = horizontal;
                    Debug.Log("Horizontal sent to Server: ");
                    SendHorizontalInputServerRpc(horizontal);
                }
                break;

            case PlayerRole.Pedal:
                bool pedaling = LocalInputReader.Instance.IsPressingSpace;
                if (pedaling != _lastPedaling)
                {
                    _lastPedaling = pedaling;
                    SendPedalingServerRpc(pedaling);
                }

                // bool braking = InputReader.Instance.IsPressingLeftShift;
                // if (braking != _lastBraking)
                // {
                //     _lastBraking = braking;
                //     SendBrakingServerRpc(braking);
                // }
                break;
        }
    }

    [Rpc(SendTo.Server)]
    private void SendHorizontalInputServerRpc(float value)
    {
        Debug.Log("Horizontal input received from Server: " );
        _currentVehicle.SetHorizontalInput(value);
    }

    [Rpc(SendTo.Server)]
    private void SendPedalingServerRpc(bool value)
    {
        _currentVehicle.SetPedaling(value);
    }

    // [Rpc(SendTo.Server)]
    // private void SendBrakingServerRpc(bool value)
    // {
    //     CurrentVehicle.SetBraking(value);
    // }
}