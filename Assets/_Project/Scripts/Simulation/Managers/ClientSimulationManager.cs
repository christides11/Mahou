using Mahou.Input;
using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public class ClientSimulationManager : SimulationManagerBase
    {
        // Delegate for adjusting the simulation speed based on incoming state data.
        public ClientSimulationAdjuster clientSimulationAdjuster;

        // Queue for incoming world states.
        private Queue<ServerWorldStateMessage> worldStateQueue = new Queue<ServerWorldStateMessage>();

        // Average of the excess size the of incoming world state queue, after tick processing.
        private MovingAverage excessWorldStateAvg = new MovingAverage(10);

        public ClientManager localClient;

        /// <summary>
        /// The last tick that was received from the server and ackowledged.
        /// </summary>
        public int latestAckedServerWorldStateTick = 0;
        public int latestAckedInput = 0;

        // Tracks the last acked server tick on each client stick.
        private int[] localClientWorldTickSnapshots = new int[1024];

        /// <summary>
        /// Snapshots of player states. Index is each player's connection ID.
        /// </summary>
        private Dictionary<int, ClientSimState[]> clientStateSnapshots = new Dictionary<int, ClientSimState[]>();

        private Dictionary<int, ClientInput[]> clientInputSnapshots = new Dictionary<int, ClientInput[]>();
        private Dictionary<int, int> clientLatestAckedInput = new Dictionary<int, int>();
        private int latestAckedInputsTick = 0;

        /// <summary>
        /// Snapshots of the game state.
        /// </summary>
        private GameModeBaseSimState[] gameModeSimStateSnapshots;

        public float moveFalloff = 0.1f;

        public bool worldStateRollback = true;
        public bool inputRollback = true;
        public bool forceRollback = false;

        public Queue<ClientInput> inputQueue = new Queue<ClientInput>();

        public ClientSimulationManager(ClientManager localClient, LobbyManager lobbyManager, int gottenServerTick) : base(lobbyManager)
        {
            this.localClient = localClient;
            gameManager = GameManager.current;
            rollbackRequired = false;

            // Setup the simulation adjuster, this delegate will be responsible for time-warping the client
            // simulation whenever we are too far ahead or behind the server simulation.
            simulationAdjuster = clientSimulationAdjuster = new ClientSimulationAdjuster(gameManager.GameSettings.serverWorldStateSendRate, 3);

            gameModeSimStateSnapshots = new GameModeBaseSimState[circularBufferSize];
            localClientWorldTickSnapshots = new int[circularBufferSize];
            OnClientJoin(localClient);

            // Set the last-acknowledged server tick.
            latestAckedServerWorldStateTick = gottenServerTick;

            // Guess the tick we should be on based on our latency.
            currentRealTick = clientSimulationAdjuster.DetermineStartTick(gottenServerTick, (float)Mirror.NetworkTime.rtt, 1.0f / gameManager.GameSettings.simulationRate);

            NetworkClient.RegisterHandler<ServerWorldStateMessage>(EnqueueWorldState);
            NetworkClient.RegisterHandler<ServerClientInputMessage>(EnqueueClientInput);
        }

        private void OnClientJoin(ClientManager cm)
        {
            clientInputSnapshots.Add(cm.clientID, new ClientInput[circularBufferSize]);
            clientStateSnapshots.Add(cm.clientID, new ClientSimState[circularBufferSize]);
            clientLatestAckedInput.Add(cm.clientID, -1);
        }

        /// <summary>
        /// Interpolate the positions of clients between simulation ticks. 
        /// Required since if players don't have a framerate that's the same as the simulation rate,
        /// they will experience jittering of entities, especially the camera.
        /// </summary>
        protected override void InterpolateClients()
        {
            foreach (ClientManager cm in ClientManager.GetClients())
            {
                if (!clientStateSnapshots.ContainsKey(cm.clientID) || clientStateSnapshots[cm.clientID][(CurrentRealTick - 2) % circularBufferSize] == null)
                {
                    continue;
                }
                cm.Interpolate(clientStateSnapshots[cm.clientID][(CurrentRealTick - 2) % circularBufferSize],
                    clientStateSnapshots[cm.clientID][(CurrentRealTick - 1) % circularBufferSize], accumulator / simulationTickInterval);
            }
        }

        private bool rollbackRequired = false;
        protected override void Tick(float dt)
        {
            rollbackRequired = false;

            HandleLocalInput();
            ApplyLatestClientInputs();
            PredictClientInputs();
            SnapshotWorld();
            SimulateWorld(dt);
            currentRealTick++;
            ApplyLatestServerWorldState();
            
            if (rollbackRequired || forceRollback)
            {
                Rollback(latestAckedServerWorldStateTick);
            }
        }

        private void HandleLocalInput()
        {
            int bufidx = currentRealTick % circularBufferSize;
            ClientInput currentInput = localClient.GetInputs();
            if (inputDelay == 0)
            {
                clientLatestAckedInput[localClient.clientID] = currentRealTick;
                clientInputSnapshots[localClient.clientID][bufidx] = currentInput;
                localClient.SetInput(currentRealTick, clientInputSnapshots[localClient.clientID][bufidx]);
                SendInput(currentRealTick, currentInput);
                return;
            }
            if (inputQueue.Count == inputDelay)
            {
                clientInputSnapshots[localClient.clientID][bufidx] = inputQueue.Dequeue();
                localClient.SetInput(currentRealTick, clientInputSnapshots[localClient.clientID][bufidx]);
            }
            inputQueue.Enqueue(currentInput);
            SendInput(currentRealTick + inputDelay, currentInput);
            clientLatestAckedInput[localClient.clientID] = currentRealTick + inputDelay;
        }

        private void SendInput(int tick, ClientInput currentInput)
        {
            ClientInputMessage cim = new ClientInputMessage()
            {
                StartWorldTick = tick,
                Inputs = new ClientInput[] { currentInput },
                ClientWorldTickDeltas = new short[] { (short)(currentRealTick - localClientWorldTickSnapshots[currentRealTick % circularBufferSize]) }
            };
            NetworkClient.Send(cim);
        }

        public override void RequestInputDelayChange(int newInputDelay)
        {
            if(newInputDelay < inputQueue.Count)
            {
                // flush all inputs
                inputQueue.Clear();
            }else if(newInputDelay > inputQueue.Count)
            {
                while(newInputDelay > inputQueue.Count)
                {
                    inputQueue.Enqueue(localClient.GetInputs());
                }
            }
            this.inputDelay = newInputDelay;
        }

        protected override void PostUpdate()
        {
            base.PostUpdate();
            // Process the remaining world states if there are any, though we expect this to be empty?
            excessWorldStateAvg.ComputeAverage(worldStateQueue.Count);
            while (worldStateQueue.Count > 0)
            {
                ApplyLatestServerWorldState();
            }
        }

        #region Main Loop
        /// <summary>
        /// Apply all inputs received from the server, along with applying our guess
        /// on what the client(s) inputs are from their last confirmed input to the current local frame.
        /// </summary>
        private void ApplyLatestClientInputs()
        {
            while (clientInputServerQueue.Count > 0)
            {
                ApplyClientInput(clientInputServerQueue.Dequeue());
            }
            latestAckedInputsTick = currentRealTick;
            foreach(var c in clientLatestAckedInput)
            {
                latestAckedInputsTick = Mathf.Min(latestAckedInputsTick, c.Value);
            }
        }

        private void PredictClientInputs()
        {
            foreach (var c in clientLatestAckedInput)
            {
                ClientManager cm = ClientManager.GetClient(c.Key);
                if(cm == null)
                {
                    continue;
                }
                // Skip this client.
                if (cm.isLocalPlayer)
                {
                    continue;
                }
                for (int i = c.Value+1; i <= currentRealTick; i++)
                {
                    // Repeat last known input.
                    clientInputSnapshots[cm.clientID][i % circularBufferSize] = clientInputSnapshots[cm.clientID][c.Value % circularBufferSize];
                    cm.SetInput(i, clientInputSnapshots[cm.clientID][i % circularBufferSize]);
                }
            }
        }

        /// <summary>
        /// Snapshots all entity states for the current tick.
        /// </summary>
        private void SnapshotWorld()
        {
            int bufidx = currentRealTick % circularBufferSize;
            localClientWorldTickSnapshots[bufidx] = latestAckedServerWorldStateTick;
            foreach (var c in ClientManager.clientManagers.Values)
            {
                // SNAPSHOT STATE //
                clientStateSnapshots[c.clientID][bufidx] = c.GetClientSimState();

                // SNAPSHOT INPUT //
                if (c.isLocalPlayer)
                {
                    clientInputSnapshots[c.clientID][bufidx] = localClient.GetInputs();
                    continue;
                }
                
                // We have already received input for this tick.
                if (clientLatestAckedInput[c.clientID] > currentRealTick)
                {
                    continue;
                }
                // Remote clients repeat their last input.
                try
                {
                    clientInputSnapshots[c.clientID][bufidx] = clientInputSnapshots[c.clientID][(CurrentRealTick - 1) % circularBufferSize];
                    c.SetInput(currentRealTick, clientInputSnapshots[c.clientID][bufidx]);
                }
                catch
                {
                    Debug.Log($"Error: {circularBufferSize}, {CurrentRealTick}, {CurrentRealTick - 1}, {(CurrentRealTick - 1) % circularBufferSize}, {bufidx}");
                }
            }
        }
        #endregion

        #region Inputs
        public Queue<ServerClientInputMessage> clientInputServerQueue = new Queue<ServerClientInputMessage>();
        private void EnqueueClientInput(ServerClientInputMessage arg2)
        {
            clientInputServerQueue.Enqueue(arg2);
        }

        private void ApplyClientInput(ServerClientInputMessage msg)
        {
            ClientManager cm = msg.clientManagerIdentity.GetComponent<ClientManager>();
            if (!clientInputSnapshots.ContainsKey(cm.clientID))
            {
                OnClientJoin(cm);
            }

            for (int i = 0; i < msg.inputMessage.Inputs.Length; i++)
            {
                int inputTick = msg.inputMessage.StartWorldTick + i;
                if (inputTick <= currentRealTick)
                {
                    if (rollbackRequired == false 
                        && ClientInput.IsDifferent(msg.inputMessage.Inputs[i], clientInputSnapshots[cm.clientID][inputTick % circularBufferSize]) && inputRollback)
                    {
                        rollbackRequired = true;
                    }
                }
                clientInputSnapshots[cm.clientID][inputTick % circularBufferSize] = msg.inputMessage.Inputs[i];
                cm.SetInput(inputTick, clientInputSnapshots[cm.clientID][inputTick % circularBufferSize]);
            }
            clientLatestAckedInput[cm.clientID] = Mathf.Max(clientLatestAckedInput[cm.clientID], msg.inputMessage.StartWorldTick + msg.inputMessage.Inputs.Length - 1);
        }
        #endregion

        #region World State
        /// <summary>
        /// Enqueues a world state received by the server. It will be dequeued during the client's tick.
        /// </summary>
        /// <param name="connectionToServer"></param>
        /// <param name="serverWorldState"></param>
        private void EnqueueWorldState(ServerWorldStateMessage serverWorldState)
        {
            worldStateQueue.Enqueue(serverWorldState);
        }

        public int serverTickLead = 0;
        public int localTickLead = 0;
        /// <summary>
        /// In this method we compare a world state received from the server to what we 
        /// calculated for that tick. If it's too far off, we'll rollback and replay up
        /// to the current tick.
        /// </summary>
        private void ApplyLatestServerWorldState()
        {
            // No world states received.
            if (worldStateQueue.Count < 1)
            {
                return;
            }

            ServerWorldStateMessage incomingState = worldStateQueue.Dequeue();
            latestAckedServerWorldStateTick = incomingState.worldSnapshot.currentTick;
            latestAckedInput = incomingState.clientLatestAckedInput;
            NetworkClient.Send(new ClientSimStateMessage() { latestAckedServerWorldStateTick = latestAckedServerWorldStateTick }, 0);

            // Calculate our actual tick lead on the server perspective. We add one because the world
            // state the server sends to use is always 1 higher than the latest input that has been
            // processed.
            if (latestAckedInput > 0)
            {
                serverTickLead = latestAckedInput - latestAckedServerWorldStateTick + 1 - inputDelay;
                clientSimulationAdjuster.NotifyActualTickLead(serverTickLead);
            }

            // Get our current lead on the server.
            localTickLead = currentRealTick - latestAckedServerWorldStateTick;

            bool serverAhead = false;
            // Check if we are behind the server.
            if (incomingState.worldSnapshot.currentTick > currentRealTick)
            {
                serverAhead = true;
            }

            CheckSimulationState(incomingState, serverAhead);
        }

        /// <summary>
        /// Check for large differences in the received simulation state and our current view of that state.
        /// </summary>
        /// <param name="worldState"></param>
        private void CheckSimulationState(ServerWorldStateMessage worldState, bool serverAhead)
        {
            // We are ahead of the server's simulation, which means that we have been predicting other client's inputs.
            // We need to check if our prediction was wrong, and rollback if so.
            if (serverAhead == false)
            {
                // Check if we need to do a rollback.
                int bufidx = worldState.worldSnapshot.currentTick % circularBufferSize;
                for (int i = 0; i < worldState.worldSnapshot.clientStates.Count; i++)
                {
                    ClientSimState c = worldState.worldSnapshot.clientStates[i];
                    ClientManager cm = c.networkIdentity.GetComponent<ClientManager>();
                    // CHECK FOR ERROR //
                    bool simulationDiverged = cm.CompareSimulationStates(c, clientStateSnapshots[cm.clientID][bufidx]);
                    if (rollbackRequired == false && simulationDiverged && worldStateRollback)
                    {
                        rollbackRequired = true;
                        break;
                    }
                }
            }

            // Apply the correct state, weither we need to rollback or not.
            ApplyGameModeSimulationState(worldState);
            ApplyClientSimulationStates(worldState);
        }

        private void ApplyGameModeSimulationState(ServerWorldStateMessage incomingState)
        {
            int bufidx = incomingState.worldSnapshot.currentTick % circularBufferSize;

            gameModeSimStateSnapshots[bufidx] = incomingState.worldSnapshot.gameModeSimState;
        }

        private void ApplyClientSimulationStates(ServerWorldStateMessage incomingState)
        {
            int bufidx = incomingState.worldSnapshot.currentTick % circularBufferSize;

            // Apply the client states on the rollback frame.
            for (int i = 0; i < incomingState.worldSnapshot.clientStates.Count; i++)
            {
                ClientManager cm = incomingState.worldSnapshot.clientStates[i].networkIdentity.GetComponent<ClientManager>();
                if (!clientStateSnapshots.ContainsKey(cm.clientID))
                {
                    clientStateSnapshots.Add(cm.clientID, new ClientSimState[circularBufferSize]);
                }
                clientStateSnapshots[cm.clientID][bufidx] = incomingState.worldSnapshot.clientStates[i];

                /*if(clientStateSnapshots[cm.clientID][bufidx].playersStates != null
                    && clientStateSnapshots[cm.clientID][bufidx].playersStates.Count > 0)
                {
                    Debug.Log(incomingState.worldSnapshot.clientStates[i].playersStates[0].GetType().FullName);
                }*/
            }
        }
        #endregion

        #region Rollback
        private void Rollback(int startFrame)
        {
            currentRollbackTick = startFrame;
            if(currentRollbackTick < latestAckedServerWorldStateTick)
            {
                currentRollbackTick = latestAckedServerWorldStateTick;
            }
            IsRollbackFrame = true;
            int bufidx = currentRollbackTick % circularBufferSize;

            // RESTORE GAME STATE //
            gameManager.GameMode.ApplySimState(gameModeSimStateSnapshots[bufidx]);
            foreach (var cm in ClientManager.clientManagers)
            {
                cm.Value.ApplyClientSimState(clientStateSnapshots[cm.Key][bufidx]);
            }

            // ADVANCE FORWARD //
            while (currentRollbackTick < currentRealTick)
            {
                bufidx = currentRollbackTick % circularBufferSize;

                // APPLY STATE & INPUT //
                gameManager.GameMode.ApplySimState(gameModeSimStateSnapshots[bufidx]);
                foreach (var cm in ClientManager.clientManagers)
                {
                    // Rewrite the historical state snapshot.
                    clientStateSnapshots[cm.Key][bufidx] = cm.Value.GetClientSimState();
                }

                // SIMULATE //
                SimulateWorld(simulationTickInterval);
                currentRollbackTick++;
            }
            IsRollbackFrame = false;
            SimulationAudioManager.Cleanup();
        }
        #endregion
    }
}