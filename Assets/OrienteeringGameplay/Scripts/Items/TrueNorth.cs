using UnityEngine;

namespace VentRout.Orienteering.Gameplay
{
    public class TrueNorth : MonoBehaviour
    {
        public static TrueNorth Instance
        {
            get;
            private set;
        }

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }
    }
}