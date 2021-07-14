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
using UnityEngine.SceneManagement;

namespace Mahou.Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager current;

        public delegate void ClientAction(int clientID);
        public event ClientAction OnClientJoined;
        public delegate void LobbyInfoAction(LobbyManager lobbyManager);
        public event LobbyInfoAction OnLobbyInfoChanged;

        public MatchManager MatchManager { get { return matchManager; } }
        public LobbySettings Settings { get { return settings; } }

        public bool matchInProgress = false;

        [Header("Referneces")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Mahou.Networking.NetworkManager networkManager;
        public string lobbySceneName = "";

        [Header("Settings")]
        [SerializeField] private LobbySettings settings;

        [Header("Other")]
        [SerializeField] private MatchManager matchManager = null;
        [SerializeField] private bool matchStarted = false;

        public Dictionary<int, ClientLobbyInfo> clientLobbyInfo = new Dictionary<int, ClientLobbyInfo>();

        public void Initialize()
        {
            current = this;

            NetworkServer.RegisterHandler<ClientInitMatchResultMessage>(ServerOnReceivedInitMatchResult, true);

            NetworkClient.RegisterHandler<LobbyInfoMessage>(OnReceivedLobbyInfo, false);
            NetworkClient.RegisterHandler<ServerInitMatchMessage>(ClientOnReceivedMatchInitRequest, true);
            NetworkClient.RegisterHandler<ServerStartMatchMessage>(ClientOnReceivedMatchStartRequest, true);
            NetworkClient.RegisterHandler<ServerReturnToLobbyMessage>(ClientReturnToLobby, true);

            gameManager = GameManager.current;
            networkManager = gameManager.NetworkManager;
            Mahou.Networking.NetworkManager.OnServerClientConnected += OnClientJoinedLobby;
            Mahou.Networking.NetworkManager.OnServerClientDisconnected += OnClientLeaveLobby;
            Mahou.Networking.NetworkManager.OnServerClientReady += OnClientReady;
        }

        private void Update()
        {
            if (matchStarted)
            {
                matchManager.Tick();
            }
        }

        private void ClientReturnToLobby(ServerReturnToLobbyMessage arg2)
        {
            Debug.Log("TODO: RETURN TO LOBBY");
        }

        private void OnClientJoinedLobby(NetworkConnection clientConnection)
        {
            if (matchInProgress == true)
            {
                clientConnection.Disconnect();
                return;
            }

            LobbyInfoMessage msg = new LobbyInfoMessage(settings, clientLobbyInfo);
            clientConnection.Send(msg);
        }

        private void OnClientLeaveLobby(NetworkConnection clientConnection)
        {
            //clientLobbyInfo.Remove(clientConnection.connectionId);
            if (matchInProgress)
            {

            }
            else
            {

            }

            //LobbyInfoMessage msg = new LobbyInfoMessage(settings, clientLobbyInfo);
            //NetworkServer.SendToAll(msg);
        }

        private void OnClientReady(NetworkConnection clientConnection, ClientManager clientManager)
        {
            clientLobbyInfo.Add(clientManager.clientID, new ClientLobbyInfo()
            {
                clientManager = clientManager,
                initMatchSuccess = false
            });
            OnClientJoined?.Invoke(clientManager.clientID);

            LobbyInfoMessage msg = new LobbyInfoMessage(settings, clientLobbyInfo);
            NetworkServer.SendToAll(msg);
        }

        public void SetLobbySettings(LobbySettings settings)
        {
            this.settings = settings;

            LobbyInfoMessage msg = new LobbyInfoMessage(settings, clientLobbyInfo);
            NetworkServer.SendToAll(msg);
        }

        private void OnReceivedLobbyInfo(LobbyInfoMessage msg)
        {
            this.settings = msg.lobbySettings;
            this.clientLobbyInfo = msg.clientInfo;
            networkManager.ClientReadyConnection();
            OnLobbyInfoChanged?.Invoke(this);
        }

        bool serverInitFinished = false;

        #region Match Initialization
        public async UniTask InitializeMatch()
        {
            if(NetworkServer.active == false)
            {
                return;
            }
            matchInProgress = true;

            ServerSimulationManager ssm = new ServerSimulationManager(this);
            matchManager = new MatchManager(GameManager.current, this, ssm);

            NetworkServer.SendToAll(new ServerInitMatchMessage());
            
            bool initResult = await InitMatch();
            if(initResult == false)
            {
                NetworkServer.SendToAll(new ServerReturnToLobbyMessage());
                return;
            }

            await SceneManager.UnloadSceneAsync(lobbySceneName);
            serverInitFinished = true;
            TryStartMatch();
        }

        private async void ClientOnReceivedMatchInitRequest(ServerInitMatchMessage msg)
        {
            if (NetworkClient.ready == false)
            {
                NetworkClient.Disconnect();
                return;
            }

            if (NetworkServer.active)
            {
                NetworkClient.Send(new ClientInitMatchResultMessage(ClientInitMatchResultMessage.InitMatchResult.SUCCESS));
                return;
            }

            ClientSimulationManager csm = new ClientSimulationManager(NetworkClient.connection.identity.GetComponent<ClientManager>(), this);
            matchManager = new MatchManager(GameManager.current, this, csm);

            bool initResult = await InitMatch();
            if (initResult == false)
            {
                NetworkClient.Send(new ClientInitMatchResultMessage(ClientInitMatchResultMessage.InitMatchResult.FAILED));
                return;
            }
            NetworkClient.Send(new ClientInitMatchResultMessage(ClientInitMatchResultMessage.InitMatchResult.SUCCESS));
        }

        private void ServerOnReceivedInitMatchResult(NetworkConnection arg1, ClientInitMatchResultMessage arg2)
        {
            clientLobbyInfo[arg1.connectionId].initMatchSuccess = true;
            if(arg2.result == ClientInitMatchResultMessage.InitMatchResult.FAILED)
            {
                arg1.Disconnect();
            }

            TryStartMatch();
        }

        private async UniTask<bool> InitMatch()
        {
            bool gamemodeResult = await GameManager.current.SetGamemode(settings.selectedGamemode);
            if (gamemodeResult == false)
            {
                return false;
            }

            bool setupResult = await GameManager.current.GameMode.SetupGamemode(GameManager.current.CurrentGamemode.GameModeComponentReferences, settings.requiredContent);
            if (setupResult == false)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Match Start
        private void TryStartMatch()
        {
            if(serverInitFinished == false)
            {
                return;
            }

            foreach (var c in clientLobbyInfo.Values)
            {
                if (c.initMatchSuccess == false)
                {
                    return;
                }
            }

            Debug.Log("All players ready, starting match.");
            foreach (var c in clientLobbyInfo.Values)
            {
                matchManager.clientMatchInfo.Add(c.clientManager.connectionToClient.connectionId, new ClientMatchInfo()
                {
                    connectionID = c.clientManager.connectionToClient.connectionId,
                    clientManager = c.clientManager
                });
            }

            matchManager.ServerStartMatch();
            matchStarted = true;

            NetworkServer.SendToAll(new ServerStartMatchMessage(matchManager.SimulationManager.CurrentRealTick));
        }

        private void ClientOnReceivedMatchStartRequest(ServerStartMatchMessage arg2)
        {
            if (NetworkServer.active)
            {
                return;
            }
            matchManager.ClientStartMatch(arg2.worldTick);
            matchStarted = true;
        }
        #endregion
    }
}
