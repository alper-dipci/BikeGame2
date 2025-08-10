using System;
using NaughtyAttributes;
using rayzngames;
using UnityEngine;

namespace DefaultNamespace
{
    public class InputReader : SingletonMonoBehaviour<InputReader>
    {
        public Action OnEscapeButtonPressed;
        private BicycleVehicle _bicycleVehicle;

        private void Start()
        {
            _bicycleVehicle = FindFirstObjectByType<BicycleVehicle>();
            if (_bicycleVehicle == null)
            {
                Debug.LogError("BicycleVehicle not found in the scene.");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapeButtonPressed?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                OnResetButtonPressed();
            }
        }
        private void OnResetButtonPressed ()
        {
            if (_bicycleVehicle != null)
            {
                _bicycleVehicle.ResetToLastCheckPoint();
            }
            else
            {
                Debug.LogError("BicycleVehicle is not assigned.");
            }
        }
    }
}