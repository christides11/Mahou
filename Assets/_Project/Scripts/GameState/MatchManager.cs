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
        public delegate void EmptyAction();
        public event EmptyAction OnMatchStarted;

        public SimulationManagerBase SimulationManager { get { return simManager; } }

        [SerializeReference] private SimulationManagerBase simManager;
        public GameManager gameManager;
        public LobbyManager lobbyManager;
        public NetworkManager networkManager;

        /// <summary>
        /// Information on clients that have joined the game.
        /// Only server-side.
        /// </summary>
        public Dictionary<int, ClientMatchInfo> clientMatchInfo = new Dictionary<int, ClientMatchInfo>();

        public MatchManager(GameManager gameManager, LobbyManager lobbyManager, SimulationManagerBase simManager)
        {
            this.gameManager = gameManager;
            this.lobbyManager = lobbyManager;
            this.simManager = simManager;
            this.networkManager = gameManager.NetworkManager;
        }

        public void Tick()
        {
            if (simManager == null)
            {
                return;
            }
            simManager.Update(Time.deltaTime);
        }

        public void ServerStartMatch()
        {
            (SimulationManager as ServerSimulationManager).StartMatch();
            OnMatchStarted?.Invoke();
        }

        public void ClientStartMatch(int startTick)
        {
            (SimulationManager as ClientSimulationManager).StartMatch(startTick);
            OnMatchStarted?.Invoke();
        }
    }
}