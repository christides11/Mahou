using CAF.Simulation;
using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mahou.Simulation
{
    public class ServerSimulationManager : SimulationManagerBase
    {
        private GameManager gameManager;

        // Handles storing inputs received by clients.
        private ClientInputProcessor clientInputProcessor;

        /// <summary>
        /// This timer handles when the server should send the world state to the clients.
        /// This is independent of the simulation rate, so it needs it's own timer.
        /// </summary>
        [SerializeField] private FixedTimer worldStateBroadcastTimer;

        /// <summary>
        /// Snapshots of player states. Used when rolling back the simulation. Index is each player's connection ID.
        /// </summary>
        [SerializeField] private Dictionary<int, ClientSimState[]> playerStateSnapshots = new Dictionary<int, ClientSimState[]>();

        // Reusable hash set for players whose input we've checked each frame.
        private HashSet<uint> unprocessedPlayerIds = new HashSet<uint>();

        // Current input struct for each player.
        private Dictionary<int, TickInput> currentClientInput = new Dictionary<int, TickInput>();

        public ServerSimulationManager(LobbyManager lobbyManager) : base(lobbyManager)
        {
            gameManager = GameManager.current;
            clientInputProcessor = new ClientInputProcessor();

            Mahou.Networking.NetworkManager.OnServerClientReady += InitializePlayerState;
            NetworkServer.RegisterHandler<ClientInputMessage>(EnqueuePlayerInput);

            // Initialize timers.
            worldStateBroadcastTimer = new FixedTimer((float)gameManager.GameSettings.serverTickRate, BroadcastWorldState);
            worldStateBroadcastTimer.Start();
        }

        private void EnqueuePlayerInput(NetworkConnection arg1, ClientInputMessage arg2)
        {
            // Enqueue input for player.
            clientInputProcessor.EnqueueInput(arg2, arg1, currentTick);

            // Update the latest input tick that we have for that client.
            lobbyManager.MatchManager.clientConnectionInfo[arg1.identity.netId].latestInputTick =
                arg2.StartWorldTick + (uint)arg2.Inputs.Length - 1;
        }

        protected override void Tick(float dt)
        {
            // HOST INPUT HANDLING //
            if (NetworkServer.localClientActive
                && NetworkServer.localConnection.identity)
            {
                ClientInputMessage cim = new ClientInputMessage();
                cim.ClientWorldTickDeltas = new short[1] { 0 };
                cim.StartWorldTick = currentTick;
                cim.Inputs = new Input.ClientInput[] { ClientManager.local ? ClientManager.local.GetInputs() : new Input.ClientInput() };
                clientInputProcessor.EnqueueInput(cim, NetworkServer.localConnection, currentTick);
            }

            // GET CLIENTS TICK INPUT //
            unprocessedPlayerIds.Clear();
            unprocessedPlayerIds.UnionWith(ClientManager.GetClientIDs());
            var tickInputs = clientInputProcessor.DequeueInputsForTick(currentTick);
            // Apply the inputs we got for this frame for all clients.
            foreach (var tickInput in tickInputs)
            {
                ClientManager player = tickInput.client.GetComponent<ClientManager>();
                currentClientInput[player.connectionToClient.connectionId] = tickInput;
                player.SetInput(tickInput.input);
                unprocessedPlayerIds.Remove(player.netId);

                // Mark the player as synchronized.
                lobbyManager.MatchManager.clientConnectionInfo[player.netId].synced = true;
            }

            // Any remaining players without inputs have their latest input command repeated,
            // but we notify them that they need to fast-forward their simulation to improve buffering.
            foreach (var clientID in unprocessedPlayerIds)
            {
                // If the player is not yet synchronized, this means that they haven't sent any
                // inputs yet. Ignore them for now.
                if (!lobbyManager.MatchManager.clientConnectionInfo.ContainsKey(clientID) ||
                    !lobbyManager.MatchManager.clientConnectionInfo[clientID].synced)
                {
                    continue;
                }

                ClientManager cManager = ClientManager.clientManagers[clientID];

                TickInput latestInput;
                if (clientInputProcessor.TryGetLatestInput(clientID, out latestInput))
                {
                    currentClientInput[cManager.connectionToClient.connectionId] = latestInput;
                    cManager.SetInput(latestInput.input);
                }
                else
                {
                    Debug.Log($"No inputs for player #{clientID} and no history to replay.");
                }
            }

            // BROADCAST INPUTS //
            BroadcastInputs();

            // ADVANCE SIMULATION //
            SimulateWorld(dt);
            ++currentTick;


            // SNAPSHOT WORLD STATE //
            var bufidx = CurrentTick % circularBufferSize;
            ClientManager.GetClients().ForEach(p => {
                playerStateSnapshots[p.networkIdentity.connectionToClient.connectionId][bufidx] = p.GetClientSimState();
            });

            // BROADCAST WORLD STATE //
            worldStateBroadcastTimer.Update(dt);
        }

        private void BroadcastInputs()
        {
            List<TickInput> tInputs = new List<TickInput>();

            foreach (var k in currentClientInput.Keys)
            {
                tInputs.Add(currentClientInput[k]);
            }

            ServerStateInputMessage serverInputMsg = new ServerStateInputMessage(currentTick, tInputs);
            NetworkServer.SendToAll(serverInputMsg, 0, true);
        }

        #region Player State
        private void InitializePlayerState(NetworkConnection clientConnection, ClientManager clientManager)
        {
            playerStateSnapshots.Add(clientConnection.connectionId, new ClientSimState[1024]);
        }

        public void ResetPlayerState(ClientManager player)
        {
            InitializePlayerState(player.connectionToClient, player);
        }
        #endregion

        #region World State
        /// <summary>
        /// Send a snapshot of the world state to every client.
        /// </summary>
        /// <param name="dt"></param>
        private void BroadcastWorldState(float dt)
        {
            ServerWorldStateMessage serverStateMsg = new ServerWorldStateMessage();

            WorldSnapshot snapshot = new WorldSnapshot();
            snapshot.currentTick = currentTick;
            snapshot.clientStates = ClientManager.clientManagers.Select(c => c.Value.GetClientSimState()).ToList();
            serverStateMsg.worldSnapshot = snapshot;

            foreach(var v in lobbyManager.MatchManager.clientConnectionInfo)
            {
                // Ignore the host.
                if (NetworkServer.localClientActive
                    && NetworkServer.localConnection.connectionId == v.Value.connectionID)
                {
                    continue;
                }

                serverStateMsg.latestAckedInput = v.Value.latestInputTick;

                NetworkServer.SendToClientOfPlayer(v.Value.clientManager.networkIdentity, serverStateMsg, 1);
            }
        }
        #endregion
    }
}