using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Mahou.Managers;
using Mahou.Debugging;

namespace Mahou
{
    public class BootLoader : MonoBehaviour
    {
        public string bootloaderScene;
        public string mainMenuScene;

        public GameManager gameManager;
        [SerializeField] private ConsoleReader consoleReader;

        public bool useArgs = false;
        public List<string> args = new List<string>();

        private void Awake()
        {
            gameManager.Initialize();
        }

        async void Start()
        {
            // Unload other scenes.
            if (SceneManager.GetActiveScene().name == bootloaderScene
                && SceneManager.sceneCount > 1)
            {
                for(int i = 0; i < SceneManager.sceneCount; i++)
                {
                    if(SceneManager.GetSceneAt(i).name == bootloaderScene)
                    {
                        continue;
                    }
                    await SceneManager.UnloadSceneAsync(i);
                }
            }

            if (SceneManager.sceneCount == 1)
            {
                await SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainMenuScene));
            }

            if (useArgs)
            {
                foreach(string s in args)
                {
                    _ = consoleReader.Convert(s);
                }
            }
        }
    }
}