using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class SessionPlayerListItem : MonoBehaviour
    {
        public TextMeshProUGUI PlayerName;
        public TextMeshProUGUI PlayerRole;
        public string PlayerId;
        
        public void Initialize(string playerName, string playerId)
        {
            PlayerName.text = playerName;
            PlayerId = playerId;
        }
        public void SetRole(PlayerRole role)
        {
            PlayerRole.text = role.ToString();
        }
    }
}