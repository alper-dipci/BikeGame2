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
        }

        [Button]
        public void rotate90Degrees()
        {
                _bicycleVehicle.rotate90Degrees();
        }
        [Button]
        public void rotate180Degrees()
        {
            _bicycleVehicle.rotate180Degrees();
        }
        [Button]
        public void rotate360Degrees()
        {
            _bicycleVehicle.Rotate360Degrees();
        }
        
    }
}