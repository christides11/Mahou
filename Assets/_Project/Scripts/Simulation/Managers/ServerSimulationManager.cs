using Mahou.Managers;
using Mahou.Networking;
using Mirror;
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

        // Reusable hash set for players whose input we've checked each frame.
        private HashSet<uint> unprocessedPlayerIds = new HashSet<uint>();

        /// <summary>
        /// This timer handles when the server should send the world state to the clients.
        /// This is independent of the simulation rate, so it needs it's own timer.
        /// </summary>
        [SerializeField] private FixedTimer worldStateBroadcastTimer;

        /// <summary>
        /// Snapshots of player states. Used when rolling back the simulation. Index is each player's connection ID.
        /// </summary>
        private Dictionary<int, ClientSimState[]> clientStateSnapshots = new Dictionary<int, ClientSimState[]>();

        /// <summary>
        /// Current input struct for each player.
        /// </summary>
        private Dictionary<int, TickInput> clientCurrentInput = new Dictionary<int, TickInput>();

        public ServerSimulationManager(LobbyManager lobbyManager) : base(lobbyManager)
        {
            gameManager = GameManager.current;
            clientInputProcessor = new ClientInputProcessor();

            Mahou.Networking.NetworkManager.OnServerClientReady += InitializePlayerState;
            Mahou.Networking.NetworkManager.OnServerClientDisconnected += HandleClientDisconnect;
            NetworkServer.RegisterHandler<ClientInputMessage>(EnqueuePlayerInput);

            // Initialize timers.
            worldStateBroadcastTimer = new FixedTimer((float)gameManager.GameSettings.serverTickRate, BroadcastWorldState);
            worldStateBroadcastTimer.Start();
        }

        private void HandleClientDisconnect(NetworkConnection clientConnection)
        {

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
            HandleHostInput();

            // GET CLIENTS TICK INPUT //
            unprocessedPlayerIds.Clear();
            unprocessedPlayerIds.UnionWith(ClientManager.GetClientIDs());
            var tickInputs = clientInputProcessor.DequeueInputsForTick(currentTick);
            // Apply the inputs we got for this frame for all clients.
            foreach (var tickInput in tickInputs)
            {
                ClientManager player = tickInput.client.GetComponent<ClientManager>();
                clientCurrentInput[player.connectionToClient.connectionId] = tickInput;
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
                    clientCurrentInput[cManager.connectionToClient.connectionId] = latestInput;
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
            uint bufidx = CurrentTick % circularBufferSize;
            foreach(ClientManager cm in ClientManager.GetClients())
            {
                clientStateSnapshots[cm.networkIdentity.connectionToClient.connectionId][bufidx] = cm.GetClientSimState();
            }

            // BROADCAST WORLD STATE //
            worldStateBroadcastTimer.Update(dt);
        }

        #region Input
        private void HandleHostInput()
        {
            if (NetworkServer.localClientActive
                && NetworkServer.localConnection.identity)
            {
                ClientInputMessage cim = new ClientInputMessage();
                cim.ClientWorldTickDeltas = new short[1] { 0 };
                cim.StartWorldTick = currentTick;
                cim.Inputs = new Input.ClientInput[] { ClientManager.local ? ClientManager.local.GetInputs() : new Input.ClientInput() };
                clientInputProcessor.EnqueueInput(cim, NetworkServer.localConnection, currentTick);
            }
        }

        private void BroadcastInputs()
        {
            List<TickInput> tInputs = new List<TickInput>();

            foreach (var k in clientCurrentInput.Keys)
            {
                tInputs.Add(clientCurrentInput[k]);
            }

            ServerStateInputMessage serverInputMsg = new ServerStateInputMessage(currentTick, tInputs);
            NetworkServer.SendToAll(serverInputMsg, 0, true);
        }
        #endregion

        #region Player State
        private void InitializePlayerState(NetworkConnection clientConnection, ClientManager clientManager)
        {
            clientStateSnapshots.Add(clientConnection.connectionId, new ClientSimState[1024]);
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
            // Build Message
            ServerWorldStateMessage serverStateMsg = new ServerWorldStateMessage();
            WorldSnapshot snapshot = new WorldSnapshot();
            snapshot.currentTick = currentTick;
            List<ClientSimState> clientStates = new List<ClientSimState>();
            foreach(ClientManager cm in ClientManager.clientManagers.Values)
            {
                clientStates.Add(cm.GetClientSimState());
            }
            snapshot.clientStates = clientStates;
            serverStateMsg.worldSnapshot = snapshot;

            // Send Message
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