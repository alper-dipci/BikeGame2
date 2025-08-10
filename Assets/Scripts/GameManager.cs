using System;
using rayzngames;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        protected override bool _isPermanent => false;
        [SerializeField] private BicycleVehicle bicycle;

        public Action OnPlayerFailed; 

        public Transform GetCurrentVehicle()
        {
            return bicycle.transform;
        }

        public void DeadZoneTriggered()
        {
            OnPlayerFailed?.Invoke();
        }
    }
}