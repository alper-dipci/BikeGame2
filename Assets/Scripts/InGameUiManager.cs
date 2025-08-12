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
        [SerializeField] Button ResetToLastCheckPointButton;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            if(GameManager.Instance.IsMultiplayer && !NetworkManager.Singleton.IsServer)
                return;
            LocalInputReader.Instance.OnEscapeButtonPressed += ToggleEscapeUi;
            SetMotorTorkButton.onClick.AddListener(SetMotorTork);
            ResetToLastCheckPointButton.onClick.AddListener(ResetToLastCheckPoint);
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
            Cursor.lockState = EscapeUiContainer.gameObject.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void Update()
        {
            SpeedText.text = speedPrefix + bicycle.currentSpeed.ToString("F1") ;
        }
        private void ResetToLastCheckPoint()
        {
            bicycle.ResetToLastCheckPoint();
        }
    }
}