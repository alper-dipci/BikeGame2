using System;
using Unity.Netcode;
using UnityEngine;

namespace DefaultNamespace
{
    public class VehicleTrigger : MonoBehaviour
    {
        private void OnEnable()
        {
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            switch (other.tag)
            {
                case "CheckPoint":
                    CheckPointManager.Instance.SetLastCheckPointPosition(transform.position);
                    Debug.Log($"Checkpoint reached at position: {transform.position}");
                    break;
                case "DeadZone":
                    GameManager.Instance.DeadZoneTriggered();
                    Debug.Log("Dead zone triggered, resetting to last checkpoint.");
                    break;
                case "JumpEffect":
                    
                default:
                    break;
            }
        }
    }
}