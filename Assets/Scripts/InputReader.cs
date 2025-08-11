using System;
using NaughtyAttributes;
using rayzngames;
using UnityEngine;

namespace DefaultNamespace
{
    public class InputReader : SingletonMonoBehaviour<InputReader>
    {
        public Action OnEscapeButtonPressed;
        private BicycleVehicleNew _bicycleVehicleNew;

        private void Start()
        {
            _bicycleVehicleNew = FindFirstObjectByType<BicycleVehicleNew>();
            if (_bicycleVehicleNew == null)
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
            if (_bicycleVehicleNew != null)
            {
                _bicycleVehicleNew.ResetToLastCheckPoint();
            }
            else
            {
                Debug.LogError("BicycleVehicle is not assigned.");
            }
        }
    }
}