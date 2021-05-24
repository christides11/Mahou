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
        /// Information on clients that have joined the game.
        /// </summary>
        public Dictionary<int, ClientConnectionInfo> joinedClients
            = new Dictionary<int, ClientConnectionInfo>();

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
            joinedClients.Add(clientManager.connectionToClient.connectionId,
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