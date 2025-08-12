using System;
using UnityEngine;

namespace DefaultNamespace
{
    // allah affetsin .com
    public class SceneReferencesManager : SingletonMonoBehaviour<SceneReferencesManager>
    {
        protected override bool _isPermanent => false;
        [SerializeField] private NetworkVehicleBase bicycle;

        private void Start()
        {
            GameManager.Instance.ChangeVehicle(bicycle);
        }
    }
}