using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "WheelDb", menuName = "Vehicle/Wheel Database")]
    public class WheelDb : ScriptableObject
    {
        public WheelFrictionProfile DefaultFrictionProfile;
        public WheelFrictionProfile IceFrictionProfile;
    }
}