using System;
using NaughtyAttributes;
using rayzngames;
using UnityEngine;

namespace DefaultNamespace
{
    public class LocalInputReader : SingletonMonoBehaviour<LocalInputReader>
    {
        //yeni input sistemine geçirilcek bi ara
        public Action OnEscapeButtonPressed;
        public Action OnResetButtonPressed;
        public float HorizontalInput { get; private set; }
        public bool IsPressingSpace { get; private set; }
        public bool IsPressingLeftShift { get; private set; }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapeButtonPressed?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnResetButtonPressed?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                IsPressingSpace = true;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                IsPressingSpace = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                IsPressingLeftShift = true;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                IsPressingLeftShift = false;
            }

            HorizontalInput = Input.GetAxis("Horizontal");
        }
    }
}