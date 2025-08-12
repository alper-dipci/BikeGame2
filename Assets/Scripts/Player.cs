using Unity.Netcode;


public class Player : NetworkBehaviour
{
    public PlayerRole PlayerRole { get; private set; }

    public override void OnNetworkSpawn()
    {
        PlayerRole = SessionManager.Instance.MyRole;
        SessionManager.Instance.OnLocalPlayerRoleChanged += OnLocalPlayerRoleChanged;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.OnLocalPlayerRoleChanged -= OnLocalPlayerRoleChanged;
        }
        base.OnNetworkDespawn();
    }

    private void OnLocalPlayerRoleChanged(PlayerRole role)
    {
        SetPlayerRole(role);
    }

    public void SetPlayerRole(PlayerRole role)
    {
        PlayerRole = role;
    }
    
}