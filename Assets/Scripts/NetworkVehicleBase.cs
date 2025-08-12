using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;

public abstract class NetworkVehicleBase : NetworkBehaviour
{
    [SerializeField] protected Rigidbody rb;
    
    private NetworkVariable<float> _horizontalInput = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> _isPedaling = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> _isBraking = new(writePerm: NetworkVariableWritePermission.Server);

    protected float HorizontalInput => GameManager.Instance.IsSinglePlayer
        ? LocalInputReader.Instance.HorizontalInput
        : _horizontalInput.Value;

    protected bool IsPedaling => GameManager.Instance.IsSinglePlayer
        ? LocalInputReader.Instance.IsPressingSpace
        : _isPedaling.Value;

    protected bool IsBraking => GameManager.Instance.IsSinglePlayer
        ? LocalInputReader.Instance.IsPressingLeftShift
        : _isBraking.Value;

    public virtual void SetHorizontalInput(float value)
    {
        _horizontalInput.Value = value;
    }

    public virtual void SetPedaling(bool isPedaling)
    {
        _isPedaling.Value = isPedaling;
    }

    public virtual void SetBraking(bool isBraking)
    {
        _isBraking.Value = isBraking;
    }

    public void OnJumpColliderTriggered()
    {
        rb.AddForce(Vector3.up * 1000f, ForceMode.Impulse);
    }
}