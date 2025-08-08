using System;
using rayzngames;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class InGameUiManager : SingletonMonoBehaviour<InGameUiManager>
    {
        protected override bool _isPermanent => false;
        [SerializeField] private BicycleVehicle bicycle;
        [SerializeField] private string speedPrefix = "Speed = ";
        [SerializeField] TextMeshProUGUI SpeedText;
        
        [SerializeField] Transform EscapeUiContainer;
        [SerializeField] TMP_InputField MotorTorkInputField;
        [SerializeField] Button SetMotorTorkButton;

        private void Start()
        {
            if(!NetworkManager.Singleton.IsServer)
                return;
            InputReader.Instance.OnEscapeButtonPressed += ToggleEscapeUi;
            SetMotorTorkButton.onClick.AddListener(SetMotorTork);
        }

        private void SetMotorTork()
        {
            if (float.TryParse(MotorTorkInputField.text, out var tork))
            {
                bicycle.SetInstantMotorForce(tork);
            }
        }

        private void ToggleEscapeUi()
        {
            EscapeUiContainer.gameObject.SetActive(!EscapeUiContainer.gameObject.activeSelf);
        }

        private void Update()
        {
            SpeedText.text = speedPrefix + bicycle.currentSpeed.ToString("F1") ;
        }
    }
}