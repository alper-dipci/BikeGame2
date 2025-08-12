using System;
using DefaultNamespace;
using NaughtyAttributes;
using rayzngames;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class BicycleInput : NetworkBehaviour
{
    [SerializeField] private BicycleVehicle _bicycleVehicle;
    
    private NetworkVariable<float> _horizontalInput = new NetworkVariable<float>(0f);
    private NetworkVariable<bool> _braking = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> _pedaling = new NetworkVariable<bool>(false);
    public float GetHorizontalInput()
    {
        if (GameManager.Instance.IsSinglePlayer)
            return LocalInputReader.Instance.HorizontalInput;
        return _horizontalInput.Value;
    }
    public bool IsBraking()
    {
        if (GameManager.Instance.IsSinglePlayer)
            return LocalInputReader.Instance.IsPressingLeftShift;
        return _braking.Value;
    }
    public bool IsPedaling()
    {
        if (GameManager.Instance.IsSinglePlayer)
            return LocalInputReader.Instance.IsPressingSpace;
        return _pedaling.Value;
    }
    
    [Rpc(SendTo.Server)]
    public void SetHorizontalInputRpc(float horizontalInput)
    {
        _horizontalInput.Value = horizontalInput;
    }

    [Rpc(SendTo.Server)]
    public void SetBrakingRpc(bool braking)
    {
        _braking.Value = braking;
    }
    [Rpc(SendTo.Server)]
    public void SetPedalingRpc(bool pedaling) 
    {
        _pedaling.Value = pedaling;
    }
}