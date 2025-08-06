using System;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerRoleUi :MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI steerText;
        [SerializeField] private TextMeshProUGUI wheeelText;

        private void Start()
        {
            steerText.gameObject.SetActive(false);
            wheeelText.gameObject.SetActive(false);

            switch (SessionManager.Instance.MyRole)
            {
                case PlayerRole.Steer:
                    steerText.gameObject.SetActive(true);
                    break;
                case PlayerRole.Pedal:
                    wheeelText.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}