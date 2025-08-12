using Unity.Netcode;

namespace DefaultNamespace
{
    public interface INetworkInputReader
    {
        public void SetHorizontalInputRpc(float horizontalInput);
        public void SetBrakingRpc(bool braking);
        public void SetPedalingRpc(bool pedaling);
    }
}