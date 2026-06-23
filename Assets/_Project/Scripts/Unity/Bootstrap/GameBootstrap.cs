using UnityEngine;
using SlotRogue.Core.Events;

namespace SlotRogue.Unity.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            EventBus.Clear();
            ServiceLocator.Clear();

            Debug.Log("SlotRogue: Bootstrap initialized");
        }

        public static void ResetRun()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }
    }
}
