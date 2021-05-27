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
        public virtual FighterHitboxManager HitboxManager { get { return hitboxManager; } }

        private FighterStatsManager statsManager;
        private FighterHitboxManager hitboxManager;
        public NetworkIdentity netid;
        public FighterCharacterController cc;
        public IFighterDefinition definition;
        public float movSpeed = 0.5f;

        public bool jumpHold = false;
        public int currentJump = 0;

        public MovesetDefinition[] movesets;

        public virtual void Awake()
        {
            Initialize();
            (inputManager as FighterInputManager).Initialize();
            (stateManager as FighterStateManager).Initialize();
            (physicsManager as FighterPhysicsManager).Initialize();
            (combatManager as FighterCombatManager).Initialize();
            (hurtboxManager as FighterHurtboxManager).Initialize();
            statsManager.Initialize();
            hitboxManager.Initialize();
            SetupStates();
            KinematicCharacterSystem.Settings.AutoSimulation = false;
            KinematicCharacterSystem.Settings.Interpolate = false;
            movesets = definition.GetMovesets();
            CombatManager.SetMoveset(0);
            StatsManager.SetStats(movesets[0].fighterStats);
            (PhysicsManager as FighterPhysicsManager).OnGroundedChanged += (data) => { if (data == true) ResetGroundOptions(); };
        }

        public virtual void Initialize()
        {
            inputManager = GetComponent<FighterInputManager>();
            stateManager = GetComponent<FighterStateManager>();
            combatManager = GetComponent<FighterCombatManager>();
            physicsManager = GetComponent<FighterPhysicsManager>();
            hurtboxManager = GetComponent<FighterHurtboxManager>();
            hitboxManager = GetComponent<FighterHitboxManager>();
            statsManager = GetComponent<FighterStatsManager>();
            cc = GetComponent<FighterCharacterController>();
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
            (hurtboxManager as FighterHurtboxManager).Reset();
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
            Vector2 movement = InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT, frame);
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
            if (inputManager.GetButton((int)PlayerInputType.JUMP).firstPress == false)
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

        public virtual bool TryAttack()
        {
            int man = CombatManager.TryAttack();
            if (man != -1)
            {
                CombatManager.SetAttack(man);
                StateManager.ChangeState((int)FighterStates.ATTACK);
                return true;
            }
            return false;
        }

        public virtual bool TryAttack(int attackIdentifier, int attackMoveset = -1, bool resetFrameCounter = true)
        {
            if (attackIdentifier != -1)
            {
                if (attackMoveset != -1)
                {
                    CombatManager.SetAttack(attackIdentifier, attackMoveset);
                }
                else
                {
                    CombatManager.SetAttack(attackIdentifier);
                }
                StateManager.ChangeState((int)FighterStates.ATTACK, resetFrameCounter ? 0 : StateManager.CurrentStateFrame);
                return true;
            }
            return false;
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

            // Combat Manager
            simState.currentChargeLevel = combatManager.CurrentChargeLevel;
            simState.currentChargeLevelCharge = combatManager.CurrentChargeLevelCharge;
            simState.hitstop = combatManager.HitStop;
            simState.hitstun = combatManager.HitStun;
            simState.currentMoveset = combatManager.CurrentMovesetIdentifier;
            simState.currentAttackMoveset = combatManager.CurrentAttackMovesetIdentifier;
            simState.currentAttackNode = combatManager.CurrentAttackNodeIdentifier;

            // Hitbox Manager
            simState.collidedIHurtables = hitboxManager.collidedIHurtables;

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

            // Combat Manager
            combatManager.SetChargeLevel(pState.currentChargeLevel);
            combatManager.SetChargeLevelCharge(pState.currentChargeLevelCharge);
            combatManager.SetHitStop(pState.hitstop);
            combatManager.SetHitStun(pState.hitstun);
            (combatManager as FighterCombatManager).ApplySimState(pState.currentMoveset, pState.currentAttackMoveset, pState.currentAttackNode);

            // Hitbox Manager
            hitboxManager.collidedIHurtables = pState.collidedIHurtables;
        }
    }
}