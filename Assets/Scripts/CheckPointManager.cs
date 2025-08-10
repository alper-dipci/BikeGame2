using UnityEngine;

namespace DefaultNamespace
{
    public class CheckPointManager : SingletonMonoBehaviour<CheckPointManager>
    {
        protected override bool _isPermanent => false;
        public Vector3 LastCheckPointPosition { get; private set; }
        
        private void Start()
        {
            LastCheckPointPosition = Vector3.zero; // Initialize to zero or a default value
        }
        public void SetLastCheckPointPosition(Vector3 position)
        {
            LastCheckPointPosition = position;
        }
    }
}