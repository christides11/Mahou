using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mahou.Simulation
{
    public class ServerSimulationManager : SimulationManagerBase
    {
        public override int CurrentTick { get { return currentRealTick; } }

        // Handles storing inputs received by clients.
        private ClientInputProcessor clientInputProcessor;

        /// <summary>
        /// This timer handles when the server should send the world state to the clients.
        /// This is independent of the simulation rate, so it needs it's own timer.
        /// </summary>
        [SerializeField] private FixedTimer worldStateBroadcastTimer;

        // CLIENTS //
        // Reusable hash set for players whose input we've checked each frame.
        private HashSet<int> unprocessedPlayerIds = new HashSet<int>();

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
            NetworkServer.RegisterHandler<ClientInputMessage>(ReceiveClientInput);

            // Initialize timers.
            SetWorldStateBroadcastTimer((float)gameManager.GameSettings.serverWorldStateSendRate);
        }

        public virtual void SetWorldStateBroadcastTimer(float worldStateSendRate)
        {
            if(worldStateBroadcastTimer != null)
            {
                worldStateBroadcastTimer.Stop();
            }
            // Initialize timers.
            worldStateBroadcastTimer = new FixedTimer(worldStateSendRate, BroadcastWorldState);
            worldStateBroadcastTimer.Start();
        }

        protected override void InterpolateClients()
        {
            foreach (ClientManager cm in ClientManager.GetClients())
            {
                if (clientStateSnapshots[cm.clientID][(CurrentRealTick - 1) % circularBufferSize].Equals(default(ClientSimState)))
                {
                    continue;
                }
                cm.Interpolate(clientStateSnapshots[cm.clientID][(CurrentRealTick - 1) % circularBufferSize],
                    clientStateSnapshots[cm.clientID][CurrentRealTick % circularBufferSize], accumulator / simulationTickInterval);
            }
        }

        private void HandleClientDisconnect(NetworkConnection clientConnection)
        {

        }

        private void ReceiveClientInput(NetworkConnection arg1, ClientInputMessage arg2)
        {
            if (arg2.Inputs.Count() > 0)
            {
                // Enqueue input for player.
                clientInputProcessor.EnqueueInput(arg2, arg1, currentRealTick);
            }

            // Update the latest input tick that we have for that client.
            int messageInputTick = arg2.StartWorldTick + arg2.Inputs.Length - 1;
            if (lobbyManager.MatchManager.joinedClients[arg1.connectionId].latestestAckedInput < messageInputTick)
            {
                lobbyManager.MatchManager.joinedClients[arg1.connectionId].latestestAckedInput = messageInputTick;
            }

            // Send input to all clients.
            ServerClientInputMessage scInputMsg = new ServerClientInputMessage()
            {
                clientManagerIdentity = lobbyManager.MatchManager.joinedClients[arg1.connectionId].clientManager.networkIdentity,
                inputMessage = arg2
            };
            foreach (var v in lobbyManager.MatchManager.joinedClients)
            {
                if(v.Value.clientManager.networkIdentity.connectionToClient.connectionId == arg1.connectionId)
                {
                    continue;
                }
                v.Value.clientManager.networkIdentity.connectionToClient.Send(scInputMsg, 0);
            }
        }

        protected override void Tick(float dt)
        {
            HandleLocalInput();
            ApplyClientInputs();
            BroadcastUnackedInputs();
            ChangeInputDelay();
            SimulateWorld(dt);
            currentRealTick++;

            SnapshotWorld();

            // BROADCAST WORLD STATE //
            worldStateBroadcastTimer.Update(dt);
        }

        private void ChangeInputDelay()
        {
            if(requestedInputDelay == inputDelay)
            {
                return;
            }
            if(requestedInputDelay > inputDelay)
            {
                for(int i = inputDelay+1; i <= requestedInputDelay; i++)
                {
                    ClientInputMessage cim = new ClientInputMessage();
                    cim.ClientWorldTickDeltas = new short[1] { 0 };
                    cim.StartWorldTick = currentRealTick + i;
                    cim.Inputs = new Input.ClientInput[] { new Input.ClientInput() };
                    clientInputProcessor.EnqueueInput(cim, NetworkServer.localConnection, currentRealTick + i);

                    NetworkServer.SendToAll(new ServerClientInputMessage()
                    {
                        clientManagerIdentity = ClientManager.local.networkIdentity,
                        inputMessage = cim
                    }, 0);
                }

                lobbyManager.MatchManager.joinedClients[ClientManager.local.clientID].latestestAckedInput = currentRealTick + requestedInputDelay;
            }
            else
            {

            }
            inputDelay = requestedInputDelay;
        }

        private void SnapshotWorld()
        {
            int bufidx = currentRealTick % circularBufferSize;
            foreach (ClientManager cm in ClientManager.GetClients())
            {
                clientStateSnapshots[cm.clientID][bufidx] = cm.GetClientSimState();
            }
        }

        #region Simulation Objects
        public override bool RegisterSimulationObject(NetworkIdentity networkIdentity)
        {
            if(networkIdentity.TryGetComponent(out ISimObject so))
            {
                simulationObjectReferences.Add(networkIdentity, so);
                simulationObjectSnapshots.Add(networkIdentity, new ISimState[circularBufferSize]);
                return true;
            }
            return false;
        }

        public override void UnregisterSimulationObject(NetworkIdentity networkIdentity)
        {
            simulationObjectReferences.Remove(networkIdentity);
            simulationObjectSnapshots.Remove(networkIdentity);
        }
        #endregion

        #region Input
        private void HandleLocalInput()
        {
            if (NetworkServer.localClientActive == false || NetworkServer.localConnection.identity == null)
            {
                return;
            }
            // For when the input delay is set lower.
            if (lobbyManager.MatchManager.joinedClients[ClientManager.local.clientID].latestestAckedInput > currentRealTick + inputDelay)
            {
                return;
            }
            ClientInputMessage cim = new ClientInputMessage();
            cim.ClientWorldTickDeltas = new short[1] { 0 };
            cim.StartWorldTick = currentRealTick+inputDelay;
            cim.Inputs = new Input.ClientInput[] { ClientManager.local ? ClientManager.local.GetInputs() : new Input.ClientInput() };
            clientInputProcessor.EnqueueInput(cim, NetworkServer.localConnection, currentRealTick+inputDelay);

            lobbyManager.MatchManager.joinedClients[ClientManager.local.clientID].latestestAckedInput = currentRealTick + inputDelay;

            NetworkServer.SendToAll(new ServerClientInputMessage()
            {
                clientManagerIdentity = ClientManager.local.networkIdentity,
                inputMessage = cim
            }, 0);
        }

        public override void RequestInputDelayChange(int newInputDelay)
        {
            requestedInputDelay = newInputDelay;
        }

        private void ApplyClientInputs()
        {
            unprocessedPlayerIds.Clear();
            unprocessedPlayerIds.UnionWith(ClientManager.clientIDs);
            var tickInputs = clientInputProcessor.DequeueInputsForTick(currentRealTick);

            // Apply the inputs we got for this frame for all clients.
            foreach (TickInput tickInput in tickInputs)
            {
                ClientManager player = tickInput.client.GetComponent<ClientManager>();
                clientCurrentInput[player.clientID] = tickInput;
                player.SetInput(currentRealTick, clientCurrentInput[player.clientID].input);

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

                ClientManager cManager = ClientManager.GetClient(clientID);
                if(cManager == null)
                {
                    continue;
                }

                TickInput latestInput;
                if (clientInputProcessor.TryGetLatestInput(clientID, out latestInput))
                {
                    clientCurrentInput[clientID] = latestInput;
                    cManager.SetInput(currentRealTick, latestInput.input);
                }
                else
                {
                    Debug.Log($"No inputs for player #{clientID} and no history to replay.");
                }
                lobbyManager.MatchManager.joinedClients[ClientManager.local.clientID].latestestAckedInput = currentRealTick - 1;
            }
        }

        private void BroadcastUnackedInputs()
        {
            foreach (var v in lobbyManager.MatchManager.joinedClients)
            {
                if(v.Value.synced == false)
                {
                    continue;
                }
                // Client was behind on inputs, so we need to tell all clients the server's decided input.
                if (v.Value.latestestAckedInput < currentRealTick)
                {
                    v.Value.latestestAckedInput = currentRealTick;
                    ClientInputMessage cim = new ClientInputMessage()
                    {
                        ClientWorldTickDeltas = null,
                        Inputs = new Input.ClientInput[1] { clientCurrentInput[v.Value.clientManager.clientID].input },
                        StartWorldTick = currentRealTick
                    };
                    NetworkServer.SendToAll(
                        new ServerClientInputMessage() { clientManagerIdentity = v.Value.clientManager.networkIdentity, inputMessage = cim }, 
                        0, 
                        true);
                }
            }
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
                clientStates.Add(clientStateSnapshots[cm.clientID][currentRealTick % circularBufferSize]); //cm.GetClientSimState());
            }

            serverStateMsg.worldSnapshot = new WorldSnapshot()
            {
                gameModeSimState = gameManager.GameMode.GetSimState(),
                clientStates = clientStates,
                currentTick = currentRealTick
            };

            // Send Message
            foreach (var v in lobbyManager.MatchManager.joinedClients)
            {
                // Ignore the host.
                if (NetworkServer.localClientActive
                    && NetworkServer.localConnection.connectionId == v.Value.connectionID)
                {
                    continue;
                }

                serverStateMsg.clientLatestAckedInput = v.Value.latestestAckedInput;

                v.Value.clientManager.networkIdentity.connectionToClient.Send(serverStateMsg, 1);
            }
        }
        #endregion
    }
}