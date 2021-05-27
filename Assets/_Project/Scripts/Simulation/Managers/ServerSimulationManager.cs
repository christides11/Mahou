using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mahou.Simulation
{
    public class ServerSimulationManager : SimulationManagerBase
    {
        private GameManager gameManager;

        // Handles storing inputs received by clients.
        private ClientInputProcessor clientInputProcessor;

        // Reusable hash set for players whose input we've checked each frame.
        private HashSet<int> unprocessedPlayerIds = new HashSet<int>();

        /// <summary>
        /// This timer handles when the server should send the world state to the clients.
        /// This is independent of the simulation rate, so it needs it's own timer.
        /// </summary>
        [SerializeField] private FixedTimer worldStateBroadcastTimer;

        /// <summary>
        /// Snapshots of player states. Index is each player's connection ID.
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

        protected override void InterpolateClients()
        {
            foreach (ClientManager cm in ClientManager.GetClients())
            {
                if (clientStateSnapshots[cm.clientID][(CurrentTick - 1) % circularBufferSize].Equals(default(ClientSimState)))
                {
                    continue;
                }
                cm.Interpolate(clientStateSnapshots[cm.clientID][(CurrentTick - 1) % circularBufferSize],
                    clientStateSnapshots[cm.clientID][CurrentTick % circularBufferSize], accumulator / simulationTickInterval);
            }
        }

        private void HandleClientDisconnect(NetworkConnection clientConnection)
        {

        }

        private void EnqueuePlayerInput(NetworkConnection arg1, ClientInputMessage arg2)
        {
            if (arg2.Inputs.Count() > 0)
            {
                // Enqueue input for player.
                clientInputProcessor.EnqueueInput(arg2, arg1, currentTick);
            }

            // Update the latest input tick that we have for that client.
            //if (lobbyManager.MatchManager.clientConnectionInfo[arg1.connectionId].latestInputTick < arg2.StartWorldTick-1)
            //{
                lobbyManager.MatchManager.joinedClients[arg1.connectionId].latestInputTick =
                    arg2.StartWorldTick + arg2.Inputs.Length - 1;
            //}
        }

        protected override void Tick(float dt)
        {
            HandleHostInput();

            // GET CLIENTS TICK INPUT //
            unprocessedPlayerIds.Clear();
            unprocessedPlayerIds.UnionWith(ClientManager.clientIDs);
            var tickInputs = clientInputProcessor.DequeueInputsForTick(currentTick);

            // Apply the inputs we got for this frame for all clients.
            foreach (TickInput tickInput in tickInputs)
            {
                ClientManager player = tickInput.client.GetComponent<ClientManager>();
                clientCurrentInput[player.clientID] = tickInput;
                player.AddInput(clientCurrentInput[player.clientID].input);

                unprocessedPlayerIds.Remove(player.clientID);

                // Mark the player as synchronized.
                lobbyManager.MatchManager.joinedClients[player.clientID].synced = true;
            }

            // Any remaining players without inputs have their latest input command repeated,
            // but we notify them that they need to fast-forward their simulation to improve buffering.
            foreach (int clientID in unprocessedPlayerIds)
            {
                // If the player is not yet synchronized, this means that they haven't sent any
                // inputs yet. Ignore them.
                if (!lobbyManager.MatchManager.joinedClients.ContainsKey(clientID) ||
                    !lobbyManager.MatchManager.joinedClients[clientID].synced)
                {
                    continue;
                }

                ClientManager cManager = ClientManager.clientManagers[clientID];

                TickInput latestInput;
                if (clientInputProcessor.TryGetLatestInput(clientID, out latestInput))
                {
                    clientCurrentInput[clientID] = latestInput;
                    cManager.AddInput(latestInput.input);
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
            int bufidx = CurrentTick % circularBufferSize;
            foreach(ClientManager cm in ClientManager.GetClients())
            {
                clientStateSnapshots[cm.clientID][bufidx] = cm.GetClientSimState();
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
                int inputDelay = ClientManager.local ? ClientManager.local.InputDelay : 0;
                ClientInputMessage cim = new ClientInputMessage();
                cim.ClientWorldTickDeltas = new short[1] { 0 };
                cim.StartWorldTick = currentTick + inputDelay;
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
            clientStateSnapshots.Add(clientManager.clientID, new ClientSimState[1024]);
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

            List<ClientSimState> clientStates = new List<ClientSimState>();
            foreach(ClientManager cm in ClientManager.clientManagers.Values)
            {
                clientStates.Add(cm.GetClientSimState());
            }

            WorldSnapshot snapshot = new WorldSnapshot()
            {
                gameModeSimState = gameManager.GameMode.GetSimState(),
                clientStates = clientStates,
                currentTick = currentTick
            };
            serverStateMsg.worldSnapshot = snapshot;

            // Send Message
            foreach(var v in lobbyManager.MatchManager.joinedClients)
            {
                // Ignore the host.
                if (NetworkServer.localClientActive
                    && NetworkServer.localConnection.connectionId == v.Value.connectionID)
                {
                    continue;
                }

                serverStateMsg.latestAckedInput = v.Value.latestInputTick;

                v.Value.clientManager.networkIdentity.connectionToClient.Send(serverStateMsg, 1);
            }
        }
        #endregion
    }
}