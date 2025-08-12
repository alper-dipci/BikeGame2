using System;
using rayzngames;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        protected override bool _isPermanent => true;
        public bool IsMultiplayer => NetworkManager.Singleton;
        public bool IsSinglePlayer => !NetworkManager.Singleton;
        public NetworkVehicleBase CurrentVehicle { get; private set; }

        public Action OnPlayerFailed;
        public Action OnVehicleChanged;

        public void ChangeVehicle(NetworkVehicleBase newVehicle)
        {
            if (CurrentVehicle != null)
            {
                CurrentVehicle.gameObject.SetActive(false);
            }

            CurrentVehicle = newVehicle;
            CurrentVehicle.gameObject.SetActive(true);
            OnVehicleChanged?.Invoke();
        }

        public void DeadZoneTriggered()
        {
            OnPlayerFailed?.Invoke();
        }
    }
}