using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HnSF.Fighters;
using Mahou.Simulation;
using Mirror;
using KinematicCharacterController;
using Mahou.Networking;
using Mahou.Managers;

namespace Mahou.Content.Fighters
{
    public class FighterManager : FighterBase, ISimObject
    {
        public virtual FighterStatsManager StatsManager { get { return statsManager; } }

        private FighterStatsManager statsManager;
        public NetworkIdentity netid;
        public FighterCharacterController cc;
        public IFighterDefinition definition;
        public float movSpeed = 0.5f;

        public bool jumpHold = false;
        public int currentJump = 0;

        public virtual void Awake()
        {
            Initialize();
            (InputManager as FighterInputManager).Initialize();
            SetupStates();
            KinematicCharacterSystem.Settings.AutoSimulation = false;
            KinematicCharacterSystem.Settings.Interpolate = false;
            CombatManager.SetMoveset(0);
            StatsManager.SetStats(definition.GetMovesets()[0].fighterStats);
            (PhysicsManager as FighterPhysicsManager).OnGroundedChanged += (data) => { if (data == true) ResetGroundOptions(); };
        }

        public virtual void Initialize()
        {
            inputManager = GetComponent<FighterInputManager>();
            stateManager = GetComponent<FighterStateManager>();
            combatManager = GetComponent<FighterCombatManager>();
            physicsManager = GetComponent<FighterPhysicsManager>();
            hurtboxManager = GetComponent<FighterHurtboxManager>();
            statsManager = GetComponent<FighterStatsManager>();
        }
         
        public virtual void Interpolate(PlayerSimState lastState, PlayerSimState currentState, float alpha)
        {
            visual.transform.position = currentState.motorState.Position * alpha
                + lastState.motorState.Position * (1.0f - alpha);
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
            return GetMovementVector(movement.x, movement.y);
        }

        public override Vector3 GetMovementVector(float horizontal, float vertical)
        {
            Vector3 forward = (InputManager as FighterInputManager).GetCameraForward();
            Vector3 right = (InputManager as FighterInputManager).GetCameraRight();

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return forward * vertical + right * horizontal;
        }

        public virtual bool TryJump()
        {
            // No jumps available.
            if(currentJump == StatsManager.CurrentStats.jumpsAvailable)
            {
                return false;
            }
            // Jump not pressed.
            if (inputManager.GetButton(Input.Action.Jump).firstPress == false)
            {
                return false;
            }
            switch (physicsManager.IsGrounded)
            {
                case true:
                    currentJump = 1;
                    StateManager.ChangeState((ushort)FighterStates.JUMP_SQUAT);
                    return true;
                case false:
                    if(currentJump == 0)
                    {
                        currentJump = 1;
                    }
                    if(currentJump == StatsManager.CurrentStats.jumpsAvailable)
                    {
                        return false;
                    }
                    currentJump++;
                    StateManager.ChangeState((ushort)FighterStates.AIR_JUMP);
                    return true;
            }
        }

        public virtual void ResetGroundOptions()
        {
            currentJump = 0;
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

            simState.currentJump = currentJump;
            simState.isGrounded = physicsManager.IsGrounded;
            simState.jumpHold = jumpHold;
            return simState;
        }

        public void ApplySimState(ISimState state)
        {
            PlayerSimState pState = (PlayerSimState)state;
            cc.Motor.ApplyState(pState.motorState);
            physicsManager.SetGrounded(pState.isGrounded);
            currentJump = pState.currentJump;
            jumpHold = pState.jumpHold;

            (physicsManager as FighterPhysicsManager3D).forceMovement = pState.forceMovement;
            (physicsManager as FighterPhysicsManager3D).forceGravity = pState.forceGravity;
            (StateManager as FighterStateManager).ChangeState(pState.mainState, pState.mainStateFrame);
        }
    }
}