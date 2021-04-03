using CAF.Simulation;
using Mahou.Input;
using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System;
using System.Collections;
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
        private GameManager gameManager;

        /// <summary>
        /// The last tick that was received from the server and ackowledged.
        /// </summary>
        public uint lastAckedServerTick = 0;
        public uint lastAckedInput = 0;

        // Tracks the last acked server tick on each client stick.
        private uint[] localClientWorldTickSnapshots = new uint[1024];

        /// <summary>
        /// Snapshots of player states. Index is each player's connection ID.
        /// </summary>
        [SerializeField] private Dictionary<uint, ClientSimState[]> clientStateSnapshots = new Dictionary<uint, ClientSimState[]>();

        [SerializeField] private Dictionary<uint, ClientInput[]> clientInputSnapshots = new Dictionary<uint, ClientInput[]>();

        public float moveFalloff = 0.1f;

        public int inputDelay = 0;

        public ClientSimulationManager(ClientManager localClient, LobbyManager lobbyManager, uint gottenServerTick) : base(lobbyManager)
        {
            this.localClient = localClient;
            gameManager = GameManager.current;

            // Setup the simulation adjuster, this delegate will be responsible for time-warping the client
            // simulation whenever we are too far ahead or behind the server simulation.
            simulationAdjuster = clientSimulationAdjuster = new ClientSimulationAdjuster(gameManager.GameSettings.serverTickRate, 5);

            localClientWorldTickSnapshots = new uint[circularBufferSize];
            OnClientJoin(localClient);

            // Set the last-acknowledged server tick.
            lastAckedServerTick = gottenServerTick;

            // Guess the tick we should be on based on our latency.
            currentTick = clientSimulationAdjuster.DetermineStartTick(gottenServerTick, (float)Mirror.NetworkTime.rtt, 1 / gameManager.GameSettings.simulationRate);

            NetworkClient.RegisterHandler<ServerWorldStateMessage>(EnqueueWorldState);
            NetworkClient.RegisterHandler<ServerStateInputMessage>(QueueServerInputs);
        }

        protected override void Tick(float dt)
        {
            // UPDATE INPUTS FROM SERVER //
            while (serverInputMessageQueue.Count > 0)
            {
                ServerStateInputMessage ssim = serverInputMessageQueue.Dequeue();
                ApplyServerInput(ssim);
                // Assume input was held.
                if(serverInputMessageQueue.Count == 0)
                {
                    for(uint i = ssim.serverTick; i < currentTick; i++)
                    {
                        for(int w = 0; w < ssim.clientInputs.Count; w++)
                        {
                            ClientManager cm = ssim.clientInputs[w].client.GetComponent<ClientManager>();
                            // Skip this client.
                            if (cm.isLocalPlayer)
                            {
                                continue;
                            }
                            clientInputSnapshots[cm.clientID][i % circularBufferSize] = ClientInput.Falloff(ssim.clientInputs[w].input, (int)(i-ssim.serverTick), moveFalloff);
                            cm.SetInput(clientInputSnapshots[cm.clientID][i % circularBufferSize]);
                        }
                    }
                }
            }

            uint bufidx = (uint)(currentTick % circularBufferSize);

            // SNAPSHOT WORLD STATE //
            localClientWorldTickSnapshots[bufidx] = lastAckedServerTick;
            foreach (var c in ClientManager.clientManagers.Values)
            {
                clientStateSnapshots[c.clientID][bufidx] = c.GetClientSimState();
                // Local client.
                if(c.isLocalPlayer)
                {
                    clientInputSnapshots[c.clientID][bufidx] = localClient.GetInputs();
                    localClient.SetInput(clientInputSnapshots[c.clientID][bufidx]);
                }
                else
                {
                    // Remote clients repeat their last input.
                    clientInputSnapshots[c.clientID][bufidx] = clientInputSnapshots[c.clientID][(bufidx-1)%circularBufferSize];
                    c.SetInput(clientInputSnapshots[c.clientID][bufidx]);
                }
            }

            // SEND ALL UNACKED INPUTS //
            var unackedInputs = new List<ClientInput>();
            var clientWorldTickDeltas = new List<short>();

            uint inputStartTick = (currentTick - lastAckedServerTick) > 20 ? currentTick - 20 : lastAckedServerTick;
            for (uint tick = inputStartTick; tick <= currentTick; ++tick)
            {
                unackedInputs.Add(clientInputSnapshots[localClient.clientID][tick % circularBufferSize]);
                clientWorldTickDeltas.Add((short)(tick - localClientWorldTickSnapshots[tick % circularBufferSize]));
            }

            ClientInputMessage cim = new ClientInputMessage()
            {
                StartWorldTick = lastAckedServerTick,
                Inputs = unackedInputs.ToArray(),
                ClientWorldTickDeltas = clientWorldTickDeltas.ToArray()
            };
            NetworkClient.Send(cim);

            // SIMULATE WORLD //
            SimulateWorld(dt);
            ++currentTick;

            // COMPARE SIMULATION //
            UpdateWorldState();
        }

        protected override void PostUpdate()
        {
            // Process the remaining world states if there are any, though we expect this to be empty?
            excessWorldStateAvg.ComputeAverage(worldStateQueue.Count);
            while (worldStateQueue.Count > 0)
            {
                UpdateWorldState();
            }
        }

        private void OnClientJoin(ClientManager cm)
        {
            clientInputSnapshots.Add(cm.clientID, new ClientInput[1024]);
            clientStateSnapshots.Add(cm.clientID, new ClientSimState[1024]);
        }

        public Queue<ServerStateInputMessage> serverInputMessageQueue = new Queue<ServerStateInputMessage>();

        #region Inputs
        private void QueueServerInputs(NetworkConnection arg1, ServerStateInputMessage arg2)
        {
            serverInputMessageQueue.Enqueue(arg2);
        }

        private void ApplyServerInput(ServerStateInputMessage msg)
        {
            for(int i = 0; i < msg.clientInputs.Count; i++)
            {
                ClientManager cm = msg.clientInputs[i].client.GetComponent<ClientManager>();
                if (!clientInputSnapshots.ContainsKey(cm.clientID))
                {
                    OnClientJoin(cm);
                }
                clientInputSnapshots[cm.clientID][msg.serverTick % circularBufferSize] = msg.clientInputs[i].input;
            }
        }
        #endregion

        #region World State
        /// <summary>
        /// Enqueues a world state received by the server. It will be dequeued during the client's tick.
        /// </summary>
        /// <param name="connectionToServer"></param>
        /// <param name="serverWorldState"></param>
        private void EnqueueWorldState(NetworkConnection connectionToServer, ServerWorldStateMessage serverWorldState)
        {
            worldStateQueue.Enqueue(serverWorldState);
        }
        #endregion

        #region Rollback
        public int serverTickLead = 0;
        public int localTickLead = 0;
        /// <summary>
        /// In this method we compare a world state received from the server to what we 
        /// calculated for that tick. If it's too far off, we'll rollback and replay up
        /// to the current tick.
        /// </summary>
        private void UpdateWorldState()
        {
            // No world states received.
            if (worldStateQueue.Count < 1)
            {
                return;
            }

            ServerWorldStateMessage incomingState = worldStateQueue.Dequeue();
            lastAckedServerTick = incomingState.worldSnapshot.currentTick;
            lastAckedInput = incomingState.latestAckedInput;

            // Calculate our actual tick lead on the server perspective. We add one because the world
            // state the server sends to use is always 1 higher than the latest input that has been
            // processed.
            if (incomingState.latestAckedInput > 0)
            {
                serverTickLead = (int)incomingState.latestAckedInput - (int)lastAckedServerTick + 1;
                clientSimulationAdjuster.NotifyActualTickLead(serverTickLead);
            }

            // Get our current lead on the server.
            localTickLead = ((int)currentTick) - ((int)lastAckedServerTick);

            bool serverAhead = false;
            // Check if we are behind the server. If so, we need to speed up our simulation.
            if (incomingState.worldSnapshot.currentTick > currentTick)
            {
                Debug.Log("Received tick from the server that is in the future, simulation is behind.");
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
            bool correctSimulation = false;
            if (!serverAhead)
            {
                // Check if we need to do a rollback.
                uint bufidx = worldState.worldSnapshot.currentTick % circularBufferSize;
                for (int i = 0; i < worldState.worldSnapshot.clientStates.Count; i++)
                {
                    ClientSimState c = worldState.worldSnapshot.clientStates[i];
                    ClientManager cm = c.networkIdentity.GetComponent<ClientManager>();
                    // CHECK FOR ERROR //
                    bool correctPositions = cm.SimComparePositions(c, clientStateSnapshots[cm.clientID][bufidx],
                                out Vector3 err);
                    if (correctPositions)
                    {
                        correctSimulation = true;
                        break;
                    }
                }
            }

            // Apply the correct state, weither we need to rollback or not.
            ApplyClientSimulationStates(worldState);

            // Simulation diverge detected. Rollback needed.
            if (correctSimulation)
            {
                Rollback(worldState.worldSnapshot.currentTick);
            }
            
        }

        private void ApplyClientSimulationStates(ServerWorldStateMessage incomingState)
        {
            uint bufidx = incomingState.worldSnapshot.currentTick % circularBufferSize;

            // Apply the client states on the rollback frame.
            for (int i = 0; i < incomingState.worldSnapshot.clientStates.Count; i++)
            {
                var c = incomingState.worldSnapshot.clientStates[i];
                ClientManager cm = c.networkIdentity.GetComponent<ClientManager>();
                if (!clientStateSnapshots.ContainsKey(cm.clientID))
                {
                    clientStateSnapshots.Add(cm.clientID, new ClientSimState[1024]);
                }
                clientStateSnapshots[cm.clientID][bufidx] = c;
            }
        }

        private void Rollback(uint startFrame)
        {
            uint bufidx = startFrame % circularBufferSize;

            // APPLY HISTORICAL STATE //
            foreach (var cm in ClientManager.clientManagers)
            {
                cm.Value.ApplyClientSimState(clientStateSnapshots[cm.Key][bufidx]);
            }

            // ADVANCE FORWARD //
            while (startFrame < currentTick)
            {
                bufidx = startFrame % circularBufferSize;

                // APPLY STATE & INPUT //
                foreach (var cm in ClientManager.clientManagers)
                {
                    // Apply inputs to the client.
                    cm.Value.SetInput(clientInputSnapshots[cm.Key][bufidx]);

                    // Rewrite the historical state snapshot.
                    clientStateSnapshots[cm.Key][bufidx] = cm.Value.GetClientSimState();
                }

                // SIMULATE //
                SimulateWorld(simulationTickInterval);
                startFrame++;
            }
        }
        #endregion
    }
}