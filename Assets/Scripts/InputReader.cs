using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class InputReader : SingletonMonoBehaviour<InputReader>
    {
        public Action OnEscapeButtonPressed;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscapeButtonPressed?.Invoke();
            }
        }
    }
}