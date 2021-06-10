using KinematicCharacterController;
using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public abstract class SimulationManagerBase
    {
        public static SimulationManagerBase instance;
        public static bool IsRollbackFrame = false;

        public int CurrentRealTick { get { return currentRealTick; } }
        public int CurrentRollbackTick { get { return currentRollbackTick; } }
        public virtual int CurrentTick { get { return IsRollbackFrame == true ? CurrentRollbackTick : currentRealTick;  } }
        public float AdjustedInterval { get { return simulationAdjuster.AdjustedInterval; } }


        protected ISimulationAdjuster simulationAdjuster = new NoopAdjuster();

        [SerializeField] protected int currentRealTick = 0;
        [SerializeField] protected int currentRollbackTick = 0;

        protected float maximumAllowedTimestep = 0.25f;
        protected float simulationTickInterval = 1.0f / 60.0f;
        protected int circularBufferSize = 1024;

        protected float accumulator;
        protected LobbyManager lobbyManager;
        protected GameManager gameManager;

        public delegate void PostUpdateAction();
        public static event PostUpdateAction OnPostUpdate;
        public static event PostUpdateAction OnPostTick;

        public bool interpolate = true;

        // SIMULATION OBJECTS //
        protected Dictionary<NetworkIdentity, ISimObject> simulationObjectReferences = new Dictionary<NetworkIdentity, ISimObject>();
        protected Dictionary<NetworkIdentity, ISimState[]> simulationObjectSnapshots = new Dictionary<NetworkIdentity, ISimState[]>();

        public int inputDelay = 3;
        public int requestedInputDelay = 3;

        protected SimulationManagerBase(LobbyManager lobbyManager)
        {
            instance = this;
            inputDelay = 3;
            requestedInputDelay = 3;
            this.lobbyManager = lobbyManager;
            this.gameManager = GameManager.current;
            this.simulationTickInterval = 1.0f / (float)GameManager.current.GameSettings.simulationRate;
            this.circularBufferSize = 1024;
        }

        public virtual void Update(float deltaTime)
        {
            if(deltaTime >= maximumAllowedTimestep)
            {
                deltaTime = maximumAllowedTimestep;
            }

            accumulator += deltaTime;
            var adjustedTickInterval = simulationTickInterval * simulationAdjuster.AdjustedInterval;
            gameManager.GameMode.Update();
            while (accumulator >= adjustedTickInterval)
            {
                // Although we can run the simulation at different speeds, the actual tick processing is
                // *always* done with the original unmodified rate for physics accuracy.
                // This has a time-warping effect.
                Tick(simulationTickInterval);
                accumulator -= adjustedTickInterval;
                OnPostTick?.Invoke();
            }

            if (interpolate)
            {
                InterpolateClients();
            }

            PostUpdate();
        }

        protected virtual void InterpolateClients()
        {

        }

        protected virtual void PostUpdate()
        {
            OnPostUpdate?.Invoke();
        }

        protected abstract void Tick(float dt);

        protected virtual void SimulateWorld(float dt)
        {
            gameManager.GameMode.Tick();
            SimulatePlayersUpdate(dt);
            SimulateObjectsUpdate(dt);
            SimulatePhysics(dt);
            SimulatePlayersLateUpdate(dt);
            SimulateObjectsLateUpdate(dt);
        }

        /// <summary>
        /// Simulate physics.
        /// </summary>
        protected virtual void SimulatePhysics(float dt)
        {
            KinematicCharacterSystem.PreSimulationInterpolationUpdate(dt);
            KinematicCharacterSystem.Simulate(dt, KinematicCharacterSystem.CharacterMotors, KinematicCharacterSystem.PhysicsMovers);
            KinematicCharacterSystem.PostSimulationInterpolationUpdate(dt);
            Physics.Simulate(dt);
        }

        protected virtual void SimulatePlayersUpdate(float dt)
        {
            ClientManager.GetClients().ForEach(c => c.SimulatePlayersUpdate(dt));
        }

        protected virtual void SimulateObjectsUpdate(float dt)
        {
            foreach(var v in simulationObjectReferences)
            {
                v.Value.SimUpdate();
            }
        }

        protected virtual void SimulatePlayersLateUpdate(float dt)
        {
            ClientManager.GetClients().ForEach(c => c.SimulatePlayersLateUpdate(dt));
        }

        protected virtual void SimulateObjectsLateUpdate(float dt)
        {
            foreach (var v in simulationObjectReferences)
            {
                v.Value.SimLateUpdate();
            }
        }

        /// <summary>
        /// Registers an object to the simulation.
        /// </summary>
        /// <param name="simObject">The object to register to the simulation.</param>
        public virtual bool RegisterSimulationObject(NetworkIdentity networkIdentity)
        {
            if (networkIdentity.TryGetComponent(out ISimObject so))
            {
                simulationObjectReferences.Add(networkIdentity, so);
                simulationObjectSnapshots.Add(networkIdentity, new ISimState[circularBufferSize]);
                return true;
            }
            return false;
        }

        public virtual void UnregisterSimulationObject(NetworkIdentity networkIdentity)
        {
            simulationObjectReferences.Remove(networkIdentity);
            simulationObjectSnapshots.Remove(networkIdentity);
        }

        public virtual void RequestInputDelayChange(int newInputDelay)
        {

        }
    }
}