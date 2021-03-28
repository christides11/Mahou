using KinematicCharacterController;
using Mahou.Managers;
using Mahou.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public abstract class SimulationManagerBase
    {
        /// <summary>
        /// Current tick of the simulation.
        /// </summary>
        public uint CurrentTick { get { return currentTick; } }

        public float AdjustedInterval { get { return simulationAdjuster.AdjustedInterval; } }

        protected ISimulationAdjuster simulationAdjuster = new NoopAdjuster();

        [SerializeField] protected uint currentTick = 0;

        protected float simulationTickInterval = 1.0f / 60.0f;
        protected int circularBufferSize = 1024;

        /// <summary>
        /// A list of all objects in the simulation.
        /// </summary>
        protected List<ISimObject> simObjects = new List<ISimObject>();
        private float accumulator;
        protected LobbyManager lobbyManager;

        protected SimulationManagerBase(LobbyManager lobbyManager)
        {
            this.lobbyManager = lobbyManager;
            this.simulationTickInterval = 1.0f / (float)GameManager.current.GameSettings.simulationRate;
            this.circularBufferSize = 1024;
        }

        public void Update(float dt)
        {
            accumulator += dt;
            var adjustedTickInterval = simulationTickInterval * simulationAdjuster.AdjustedInterval;
            while (accumulator >= adjustedTickInterval)
            {
                accumulator -= adjustedTickInterval;

                //interpController.ExplicitFixedUpdate(adjustedTickInterval);

                // Although we can run the simulation at different speeds, the actual tick processing is
                // *always* done with the original unmodified rate for physics accuracy.
                // This has a time-warping effect.
                Tick(simulationTickInterval);
            }
            //interpController.ExplicitUpdate(dt);
            PostUpdate();
        }

        protected virtual void PostUpdate()
        {

        }

        protected abstract void Tick(float dt);

        protected virtual void SimulateWorld(float dt)
        {
            SimulatePlayersUpdate(dt);
            SimulatePhysics(dt);
            SimulatePlayersLateUpdate(dt);
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

        protected virtual void SimulatePlayersLateUpdate(float dt)
        {
            ClientManager.GetClients().ForEach(c => c.SimulatePlayersLateUpdate(dt));
        }

        /// <summary>
        /// Registers an object to the simulation.
        /// </summary>
        /// <param name="simObject">The object to register to the simulation.</param>
        public virtual void RegisterObject(ISimObject simObject)
        {
            if (simObjects.Contains(simObject))
            {
                return;
            }
            simObjects.Add(simObject);
        }

        public void RemoveObjectFromSimulation(ISimObject simObject)
        {
            if (simObjects.Contains(simObject))
            {
                simObjects.Remove(simObject);
            }
        }
    }
}