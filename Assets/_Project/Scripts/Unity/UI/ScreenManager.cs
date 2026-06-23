using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlotRogue.Unity.UI
{
    public class ScreenManager : MonoBehaviour
    {
        private static ScreenManager _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadCharacterSelect()
        {
            LoadScene("CharacterSelect");
        }

        public static void LoadMap()
        {
            LoadScene("Map");
        }

        public static void LoadBattle()
        {
            LoadScene("Battle");
        }

        public static void LoadShop()
        {
            LoadScene("Shop");
        }

        public static void LoadMainMenu()
        {
            Bootstrap.GameBootstrap.ResetRun();
            LoadScene("MainMenu");
        }
    }
}
