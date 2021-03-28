using Mahou.Networking;
using Mahou.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Managers
{
    [System.Serializable]
    public class MatchManager
    {
        public SimulationManagerBase SimulationManager { get { return simManager; } }

        [SerializeReference] private SimulationManagerBase simManager;
        public GameManager gameManager;
        public LobbyManager lobbyManager;
        public NetworkManager networkManager;

        /// <summary>
        /// Simulation info for each player, indexed by connectionID.
        /// Used by the server to keep track of each client.
        /// </summary>
        public Dictionary<uint, ClientConnectionInfo> clientConnectionInfo
            = new Dictionary<uint, ClientConnectionInfo>();

        public MatchManager(GameManager gameManager, LobbyManager lobbyManager, SimulationManagerBase simManager)
        {
            this.gameManager = gameManager;
            this.lobbyManager = lobbyManager;
            this.simManager = simManager;
            this.networkManager = gameManager.NetworkManager;

            NetworkManager.OnServerClientReady += OnClientConnectToServer;
        }

        private void OnClientConnectToServer(Mirror.NetworkConnection clientConnection, ClientManager clientManager)
        {
            clientConnectionInfo.Add(clientManager.netId,
                new ClientConnectionInfo()
                {
                    connectionID = clientConnection.connectionId,
                    clientManager = clientManager
                });
        }

        public void Tick()
        {
            if (simManager == null)
            {
                return;
            }
            simManager.Update(Time.deltaTime);
        }
    }
}