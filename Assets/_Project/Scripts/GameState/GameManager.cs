using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahou.Content;
using Mahou.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Mahou.Debugging;

namespace Mahou.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager current;
        public LobbyManager LobbyManager { get { return lobbyManager; } }
        public NetworkManager NetworkManager { get { return networkManager; } }
        public GameSettings GameSettings { get { return gameSettings; } }
        public ContentManager ModManager { get { return modManager; } }

        public IGameModeDefinition CurrentGamemode { get; protected set; } = null;
        public GameModeBase GameMode { get; protected set; } = null;

        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private ContentManager modManager;
        [SerializeField] private ModLoader modLoader;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private LobbyManager lobbyManager;

        [SerializeField] private AddressablesModDefinition coreMod;

        public virtual void Initialize()
        {
            current = this;
            HnSF.Input.GlobalInputManager.instance = new Mahou.Input.GlobalInputManager();
            modManager.Initialize();
            modLoader.Initialize();
            lobbyManager.Initialize();
            modLoader.loadedMods.Add("core", new LoadedModDefinition(null, coreMod));
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5))
            {
                QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
            }
        }

        /// <summary>
        /// Sets the current gamemode.
        /// </summary>
        /// <param name="gamemode">The gamemode to set.</param>
        /// <returns>True if successful.</returns>
        public virtual async UniTask<bool> SetGamemode(ModObjectReference gamemode)
        {
            await modManager.LoadContentDefinitions(ContentType.Gamemode, gamemode.modIdentifier);
            IGameModeDefinition gamemodeDefinition = (IGameModeDefinition)modManager.GetContentDefinition(ContentType.Gamemode, gamemode);

            if (gamemodeDefinition == null)
            {
                Debug.Log($"Can not find gamemode {gamemode.ToString()}");
                return false;
            }

            // Load components.
            for(int i = 0; i < gamemodeDefinition.GameModeComponentReferences.Length; i++)
            {
                if((await modManager.LoadContentDefinition(ContentType.GamemodeComponent, gamemodeDefinition.GameModeComponentReferences[i])) == false)
                {
                    Debug.Log($"Failed loading gamemode component {gamemodeDefinition.GameModeComponentReferences[i].ToString()}");
                    return false;
                }
                if ((await ((IGameModeComponentDefinition)modManager
                    .GetContentDefinition(ContentType.GamemodeComponent, gamemodeDefinition.GameModeComponentReferences[i]))
                    .LoadGamemodeComponent()) == false)
                {
                    Debug.Log($"Failed loading gamemode component {gamemodeDefinition.GameModeComponentReferences[i].ToString()}");
                    return false;
                }
            }

            ClearGamemode();

            bool gamemodeResult = await gamemodeDefinition.LoadGamemode();
            if (gamemodeResult == false)
            {
                Debug.Log($"Could not load gamemode {gamemode.ToString()}.");
                return false;
            }

            GameObject gameMode = Instantiate(gamemodeDefinition.GetGamemode().gameObject, transform);
            gameMode.GetComponent<GameModeBase>().Initialize();

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
            await modManager.LoadContentDefinitions(ContentType.Map, map.modIdentifier);
            IMapDefinition mapDefinition = (IMapDefinition)modManager.GetContentDefinition(ContentType.Map, map);

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
