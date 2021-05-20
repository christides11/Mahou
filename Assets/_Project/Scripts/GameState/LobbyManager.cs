using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahou.Content;
using Mahou.Networking;
using Mirror;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Mahou.Simulation;

namespace Mahou.Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public delegate void LobbySettingsAction(LobbyManager lobbyManager);
        public event LobbySettingsAction OnLobbySettingsSet;

        public MatchManager MatchManager { get { return matchManager; } }

        [SerializeField] private GameManager gameManager;
        [SerializeField] private Mahou.Networking.NetworkManager networkManager;
        [SerializeField] private LobbySettings settings;

        public string mainMenuSceneName = "";

        [SerializeField] private MatchManager matchManager = null;

        public void Initialize()
        {
            NetworkClient.RegisterHandler<LobbySettingsMessage>(OnReceivedLobbySettings, false);
            NetworkClient.RegisterHandler<MatchInitMessage>(OnReceivedMatchInitRequest, false);

            gameManager = GameManager.current;
            networkManager = gameManager.NetworkManager;
            Mahou.Networking.NetworkManager.OnServerClientConnected += SendClientLobbySettings;
            Mahou.Networking.NetworkManager.OnServerClientReady += SendMatchInitilizationMessage;
        }

        private void SendMatchInitilizationMessage(NetworkConnection clientConnection, ClientManager clientManager)
        {
            // Ignore host.
            if(NetworkServer.localClientActive
                && NetworkServer.localConnection.connectionId == clientConnection.connectionId)
            {
                return;
            }
            clientConnection.Send(new MatchInitMessage(matchManager.SimulationManager.CurrentTick));
        }

        private void OnReceivedMatchInitRequest(MatchInitMessage arg2)
        {
            ClientSimulationManager csm = new ClientSimulationManager(NetworkClient.connection.identity.GetComponent<ClientManager>(), this, arg2.worldTick);
            matchManager = new MatchManager(gameManager, this, csm);
        }

        public void Update()
        {
            if (matchManager != null)
            {
                matchManager.Tick();
            }
        }

        #region Hosting
        public void HostGame(LobbySettings settings)
        {
            _ = AsyncHostGame(settings);
        }

        public async UniTask AsyncHostGame(LobbySettings settings)
        {
            this.settings = settings;

            bool settingsResult = await ApplySettings();
            if (!settingsResult)
            {
                Debug.Log($"Couldn't load settings. {settings.selectedGamemode}, {settings.selectedMap}");
                return;
            }
            matchManager = new MatchManager(gameManager, this, new ServerSimulationManager(this));

            // Start the server and host client.
            networkManager.StartHost();
        }

        public async void DedicatedHostGame()
        {
        }
        #endregion

        #region Initialize
        private void SendClientLobbySettings(NetworkConnection clientConnection)
        {
            LobbySettingsMessage msg = new LobbySettingsMessage()
            {
                lobbySettings = settings
            };

            clientConnection.Send(msg);
        }

        private async void OnReceivedLobbySettings(LobbySettingsMessage msg)
        {
            settings = msg.lobbySettings;

            bool result = await ApplySettings();
            networkManager.ClientReadyConnection();

            OnLobbySettingsSet?.Invoke(this);
        }
        #endregion

        private async UniTask<bool> ApplySettings()
        {
            bool gamemodeLoaded = await gameManager.SetGamemode(settings.selectedGamemode);
            if (!gamemodeLoaded)
            {
                return false;
            }

            if (gameManager.CurrentGamemode.BattleSelectionRequired)
            {
                bool battleLoaded = await gameManager.ModManager.LoadBattleDefinitions();
            }

            bool mapLoaded = await gameManager.LoadMap(settings.selectedMap, new List<string>() { mainMenuSceneName });
            if (!mapLoaded)
            {
                return false;
            }
            return true;
        }
    }
}
