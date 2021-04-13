using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CAF.Fighters;
using Mahou.Simulation;
using Mirror;
using KinematicCharacterController;
using Mahou.Networking;
using Mahou.Managers;

namespace Mahou.Content.Fighters
{
    public class FighterManager : FighterBase, ISimObject
    {
        public virtual FighterStats Stats { get; protected set; }

        public NetworkIdentity netid;
        public FighterCharacterController cc;
        public float movSpeed = 0.5f;

        public bool fullHop = false;

        public virtual void Awake()
        {
            SetupStates();
            KinematicCharacterSystem.Settings.AutoSimulation = false;
            KinematicCharacterSystem.Settings.Interpolate = false;
        }

        public virtual void SetupStates()
        {

        }

        public void SimUpdate()
        {
            Tick();
        }

        public void SimLateUpdate()
        {
            LateTick();
        }

        /// <summary>
        /// Translates the movement vector based on the look transform's forward.
        /// </summary>
        /// <param name="frame">The frame we want to check the movement input for.</param>
        /// <returns>A direction vector based on the camera's forward.</returns>
        public virtual Vector3 GetMovementVector(uint frame = 0)
        {
            Vector2 movement = InputManager.GetAxis2D(Mahou.Input.Action.Movement_X, frame);
            return Vector3.forward * movement.y + Vector3.right * movement.x;
        }

        public ISimState GetSimState()
        {
            PlayerSimState simState = new PlayerSimState();
            simState.netID = netid;
            simState.motorState = cc.Motor.GetState();
            simState.forceMovement = (physicsManager as FighterPhysicsManager3D).forceMovement;
            simState.forceGravity = (physicsManager as FighterPhysicsManager3D).forceGravity;
            simState.mainState = (StateManager as FighterStateManager).CurrentState;
            simState.mainStateFrame = (StateManager as FighterStateManager).CurrentStateFrame;

            simState.isGrounded = IsGrounded;
            //simState.fullHop = fullHop;
            return simState;
        }

        public void ApplySimState(ISimState state)
        {
            PlayerSimState pState = (PlayerSimState)state;
            cc.Motor.ApplyState(pState.motorState);
            //fullHop = pState.fullHop;
            IsGrounded = pState.isGrounded;

            (physicsManager as FighterPhysicsManager3D).forceMovement = pState.forceMovement;
            (physicsManager as FighterPhysicsManager3D).forceGravity = pState.forceGravity;
            (StateManager as FighterStateManager).ChangeState(pState.mainState, pState.mainStateFrame);
        }
    }
}