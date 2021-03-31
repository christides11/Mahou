using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahou.Content;
using Mahou.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Mahou.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager current;

        public LobbyManager LobbyManager { get { return lobbyManager; } }
        public NetworkManager NetworkManager { get { return networkManager; } }
        public GameSettings GameSettings { get { return gameSettings; } }

        public IGameModeDefinition CurrentGamemode { get; protected set; } = null;
        public GameModeBase GameMode { get; protected set; } = null;

        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private ModManager modManager;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private LobbyManager lobbyManager;

        [SerializeField] private LocalModDefinition coreMod;

        public virtual void Initialize()
        {
            current = this;
            modManager.Initialize();
            modManager.mods.Add("core", coreMod);
            lobbyManager.Initialize();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5))
            {
                QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
            }
        }

        public virtual async UniTask<bool> LoadGamemode(ModObjectReference gamemode)
        {
            await modManager.LoadGamemodeDefinitions(gamemode.modIdentifier);
            IGameModeDefinition gamemodeDefinition = modManager.GetGamemodeDefinition(gamemode);

            if (gamemodeDefinition == null)
            {
                Debug.Log($"Can not find gamemode {gamemode.ToString()}");
                return false;
            }

            ClearGamemode();

            bool gamemodeResult = await gamemodeDefinition.LoadGamemode();
            if (gamemodeResult == false)
            {
                Debug.Log($"Could not load gamemode {gamemode.ToString()}.");
                return false;
            }

            GameObject gameMode = Instantiate(gamemodeDefinition.GetGamemode().gameObject, transform);
            this.GameMode = gameMode.GetComponent<GameModeBase>();
            CurrentGamemode = gamemodeDefinition;
            return true;
        }

        public void ClearGamemode()
        {
            if(GameMode == null)
            {
                return;
            }
            Destroy(GameMode.gameObject);
            GameMode = null;
            CurrentGamemode.UnloadGamemode();
            CurrentGamemode = null;
        }

        public virtual async UniTask<bool> LoadMap(ModObjectReference map, List<string> scenesToUnload = null)
        {
            await modManager.LoadMapDefinitions(map.modIdentifier);
            IMapDefinition mapDefinition = modManager.GetMapDefinition(map);

            if (mapDefinition == null)
            {
                Debug.Log($"Can not find map {map.ToString()}.");
                return false;
            }

            bool result = await modManager.LoadMap(map);
            if (!result)
            {
                Debug.Log($"Error loading map {map.ToString()}.");
                return false;
            }

            // Unloads scenes that aren't the singletons scene.
            if (scenesToUnload != null)
            {
                for (int i = 0; i < scenesToUnload.Count; i++)
                {
                    await SceneManager.UnloadSceneAsync(scenesToUnload[i]);
                }
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(mapDefinition.SceneNames[0]));
            return true;
        }
    }
}
