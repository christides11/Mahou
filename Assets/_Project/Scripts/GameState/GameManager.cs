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
        public ModManager ModManager { get { return modManager; } }

        public IGameModeDefinition CurrentGamemode { get; protected set; } = null;
        public GameModeBase GameMode { get; protected set; } = null;

        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private ModManager modManager;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private LobbyManager lobbyManager;

        [SerializeField] private AddressablesModDefinition coreMod;

        public virtual void Initialize()
        {
            current = this;
            HnSF.Input.GlobalInputManager.instance = new Mahou.Input.GlobalInputManager();
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

        /// <summary>
        /// Sets the current gamemode.
        /// </summary>
        /// <param name="gamemode">The gamemode to set.</param>
        /// <returns>True if successful.</returns>
        public virtual async UniTask<bool> SetGamemode(ModObjectReference gamemode, ModObjectReference battle = null)
        {
            await modManager.LoadGamemodeDefinitions(gamemode.modIdentifier);
            IGameModeDefinition gamemodeDefinition = modManager.GetGamemodeDefinition(gamemode);

            if (gamemodeDefinition == null)
            {
                Debug.Log($"Can not find gamemode {gamemode.ToString()}");
                return false;
            }

            IBattleDefinition battleDefinition = null;
            if (gamemodeDefinition.BattleSelectionRequired)
            {
                await modManager.LoadBattleDefinition(battle);
                battleDefinition = modManager.GetBattleDefinition(battle);
                if(battleDefinition == null)
                {
                    Debug.Log($"Could not find battle {battle.ToString()}");
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
            this.GameMode = gameMode.GetComponent<GameModeBase>();
            this.GameMode.InitGamemode(battleDefinition);
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
