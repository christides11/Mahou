using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HnSF.Fighters;
using Mahou.Simulation;
using Mirror;
using KinematicCharacterController;
using Mahou.Networking;
using Mahou.Managers;
using HnSF.Input;
using HnSF.Combat;
using Mahou.Combat;

namespace Mahou.Content.Fighters
{
    public class FighterManager : FighterBase, ISimObject
    {
        public bool ObjectEnabled { get; protected set; } = true;
        public GameObject LockonTarget { get; protected set; } = null;
        public bool LockedOn { get; protected set; } = false;
        public Vector3 LockonForward { get; protected set; } = Vector3.forward;

        public virtual FighterStatsManager StatsManager { get { return statsManager; } }
        public virtual FighterHitboxManager HitboxManager { get { return hitboxManager; } }
        public virtual HealthManager HealthManager { get { return healthManager; } }
        public virtual FighterPushboxManager PushboxManager { get { return pushboxManager; } }

        private HealthManager healthManager;
        private FighterStatsManager statsManager;
        private FighterHitboxManager hitboxManager;
        private FighterPushboxManager pushboxManager;
        public FighterAnimator fighterAnimator;
        public NetworkIdentity netid;
        public FighterCharacterController cc;
        public IFighterDefinition definition;
        public Collider coll;
        public float movSpeed = 0.5f;

        public bool charging = true;
        public bool jumpHold = false;
        public byte currentJump = 0;
        public int heldTime = 0;
        public int hangTime = 0;

        public MovesetDefinition[] movesets;

        [Header("Lock On")]
        public float softLockonRadius;
        public float lockonRadius;
        public float lockonFudging = 0.1f;
        public LayerMask lockonLayerMask;
        public LayerMask lockonVisibilityLayerMask;

        private Vector3 size;

        public void Enable()
        {
            ObjectEnabled = true;
        }

        public void Disable()
        {
            ObjectEnabled = false;
        }

        public virtual void Load()
        {

        }

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
            size = coll.bounds.size;
            healthManager.SetHealth(definition.Health);
        }

        public virtual void Initialize()
        {
            healthManager = GetComponent<HealthManager>();
            netid = GetComponent<NetworkIdentity>();
            inputManager = GetComponent<FighterInputManager>();
            stateManager = GetComponent<FighterStateManager>();
            combatManager = GetComponent<FighterCombatManager>();
            physicsManager = GetComponent<FighterPhysicsManager>();
            hurtboxManager = GetComponent<FighterHurtboxManager>();
            hitboxManager = GetComponent<FighterHitboxManager>();
            statsManager = GetComponent<FighterStatsManager>();
            cc = GetComponent<FighterCharacterController>();
            pushboxManager = GetComponent<FighterPushboxManager>();
        }

        public Vector3 visualOffset;
        public virtual void Interpolate(PlayerSimState lastState, PlayerSimState currentState, float alpha)
        {
            visual.transform.position = (currentState.motorState.Position * alpha
                + lastState.motorState.Position * (1.0f - alpha)) + visualOffset;
        }

        public virtual void SetupStates()
        {
            stateManager.AddState(new FighterStateIdle(), (ushort)FighterStates.IDLE);
            stateManager.AddState(new FighterStateWalk(), (ushort)FighterStates.WALK);
            stateManager.AddState(new FighterStateDash(), (ushort)FighterStates.DASH);
            stateManager.AddState(new FighterStateRun(), (ushort)FighterStates.RUN);
            stateManager.AddState(new FighterStateFall(), (ushort)FighterStates.FALL);
            stateManager.AddState(new FighterStateJumpSquat(), (ushort)FighterStates.JUMP_SQUAT);
            stateManager.AddState(new FighterStateJump(), (ushort)FighterStates.JUMP);
            stateManager.AddState(new FighterStateAirDash(), (ushort)FighterStates.AIR_DASH);
            stateManager.AddState(new FighterStateAirJump(), (ushort)FighterStates.AIR_JUMP);
            stateManager.AddState(new FighterStateAttack(), (ushort)FighterStates.ATTACK);

            stateManager.AddState(new FighterStateBlockHigh(), (ushort)FighterStates.BLOCK_HIGH);
            stateManager.AddState(new FighterStateBlockLow(), (ushort)FighterStates.BLOCK_LOW);
            stateManager.AddState(new FighterStateBlockAir(), (ushort)FighterStates.BLOCK_AIR);

            stateManager.AddState(new FighterStateFlinchGround(), (ushort)FighterStates.FLINCH_GROUND);
            stateManager.AddState(new FighterStateFlinchAir(), (ushort)FighterStates.FLINCH_AIR);
            stateManager.AddState(new FighterStateTumble(), (ushort)FighterStates.TUMBLE);

            stateManager.ChangeState((ushort)FighterStates.FALL);
        }

        public void SimUpdate()
        {
            (hurtboxManager as FighterHurtboxManager).Cleanup();
            pushboxManager.Cleanup();
            Tick();
        }

        public void SimLateUpdate()
        {
            LateTick();
        }

        public override void LateTick()
        {
            base.LateTick();
            pushboxManager.LateTick();
        }

        public virtual Vector3 GetCenter()
        {
            return transform.position + new Vector3(0, size.y / 2.0f, 0);
        }

        protected override void HandleLockon()
        {
            InputRecordButton lockonButton = InputManager.GetButton((int)PlayerInputType.LOCKON);

            LockedOn = false;
            if (lockonButton.released)
            {
                //lookHandler.SetLockOnTarget(null);
            }
            if (!lockonButton.isDown)
            {
                return;
            }
            LockedOn = true;

            if (lockonButton.firstPress)
            {
                PickLockonTarget();
                // No target but holding down lock on menas you lock the visuals rotation.
                LockonForward = visual.transform.forward;
                //lookHandler.SetLockOnTarget(LockonTarget?.GetComponent<EntityManager>());
            }

            // No target.
            if (LockonTarget == null)
            {
                return;
            }

            // Target out of range.
            if (Vector3.Distance(transform.position, LockonTarget.transform.position) > lockonRadius)
            {
                LockonTarget = null;
                //lookHandler.SetLockOnTarget(null);
                return;
            }

            // We have a target and they're in range, set our wanted forward direction.
            Vector3 dir = (LockonTarget.transform.position - transform.position);
            dir.y = 0;
            LockonForward = dir.normalized;
        }

        /// <summary>
        /// Picks the best soft lockon target based on distance.
        /// </summary>
        public void PickSoftlockTarget()
        {
            // If we're Hard Lockoning or looking in a specific direction, don't pick a target.
            if (LockedOn || InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT).magnitude >= InputConstants.movementThreshold)
            {
                return;
            }
            LockonTarget = null;
            Collider[] list = Physics.OverlapSphere(transform.position, softLockonRadius, lockonLayerMask);

            float closestDistance = softLockonRadius;
            foreach (Collider c in list)
            {
                // Ignore self.
                if (c.gameObject == gameObject)
                {
                    continue;
                }
                // Only objects with ILockonable can be locked on to.
                if (c.TryGetComponent(out ITargetable lockonComponent))
                {
                    if (Vector3.Distance(transform.position, c.transform.position) < closestDistance)
                    {
                        LockonTarget = c.gameObject;
                    }
                }
            }
        }

        /// <summary>
        /// Picks the best lockon target based on stick direction.
        /// </summary>
        private void PickLockonTarget()
        {
            LockonTarget = null;
            Collider[] list = Physics.OverlapSphere(GetCenter(), lockonRadius, lockonLayerMask);
            // The direction of the lockon defaults to the forward of the camera.
            Vector3 referenceDirection = GetMovementVector(0, 1);
            // If the movement stick is pointing in a direction, then our lockon should
            // be based on that angle instead.
            Vector2 movementDir = InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT);
            if (movementDir.magnitude >= InputConstants.movementThreshold)
            {
                referenceDirection = GetMovementVector(movementDir.x, movementDir.y);
            }

            // Loop through all targets and find the one that matches the angle the best.
            GameObject closestTarget = null;
            float closestAngle = -1.1f;
            float closestDistance = Mathf.Infinity;
            foreach (Collider c in list)
            {
                // Ignore self.
                if (c.gameObject == gameObject)
                {
                    continue;
                }

                // Only objects with ILockonable can be locked on to.
                if (c.TryGetComponent(out ITargetable targetLockonComponent))
                {
                    // The target can not be locked on to right now.
                    if (!targetLockonComponent.Targetable)
                    {
                        continue;
                    }
                    Vector3 targetDistance = targetLockonComponent.GetGameObject().GetComponent<FighterManager>().GetCenter() - GetCenter();
                    // If we can't see the target, it can not be locked on to.
                    if (Physics.Raycast(GetCenter(), targetDistance.normalized, out RaycastHit h, targetDistance.magnitude, lockonVisibilityLayerMask))
                    {
                        continue;
                    }

                    targetDistance.y = 0;
                    float currAngle = Vector3.Dot(referenceDirection, targetDistance.normalized);
                    bool withinFudging = Mathf.Abs(currAngle - closestAngle) <= lockonFudging;
                    // Targets have similar positions, choose the closer one.
                    if (withinFudging)
                    {
                        if (targetDistance.sqrMagnitude < closestDistance)
                        {
                            closestTarget = c.gameObject;
                            closestAngle = currAngle;
                            closestDistance = targetDistance.sqrMagnitude;
                        }
                    }
                    // Target is closer to the angle than the last one, this is the new target.
                    else if (currAngle > closestAngle)
                    {
                        closestTarget = c.gameObject;
                        closestAngle = currAngle;
                        closestDistance = targetDistance.sqrMagnitude;
                    }
                }
            }

            if (closestTarget != null)
            {
                LockonTarget = closestTarget;
            }

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
            if (inputManager.GetButton((int)PlayerInputType.JUMP, 0, true).firstPress == false)
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

        public virtual bool TryBlock()
        {
            if (inputManager.GetButton((int)PlayerInputType.BLOCK).isDown == true)
            {
                StateManager.ChangeState((int)FighterStates.BLOCK_HIGH);
                return true;
            }
            return false;
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

        public override void RotateVisual(Vector3 direction, float speed)
        {
            Vector3 newDirection = Vector3.RotateTowards(visual.transform.forward, direction, speed * Time.fixedDeltaTime, 0.0f);
            visual.transform.rotation = Quaternion.LookRotation(newDirection);
        }

        public AnimationClip GetAnimationClip(string animationName, int movesetIdentifier = -1)
        {
            if (movesetIdentifier == -1)
            {
                return null;
                //return definition.sharedAnimations.GetAnimation(animationName);
            }
            else
            {
                if (movesets.Length <= movesetIdentifier)
                {
                    return null;
                }
                if (movesets[movesetIdentifier].animationCollection.TryGetAnimation(animationName, out AnimationClip movesetClip))
                {
                    return movesetClip;
                }
                return null;
                //return entityDefinition.sharedAnimations.GetAnimation(animationName);
            }
        }

        public virtual ISimState GetSimState()
        {
            Debug.Log("Base call.");
            return null;
            //PlayerSimState simState = new PlayerSimState();
            //FillSimState(simState);
            //return simState;
        }

        protected virtual void FillSimState(PlayerSimState simState)
        {
            simState.objectEnabled = ObjectEnabled;
            simState.visualRotation = visual.transform.eulerAngles.y;
            simState.networkIdentity = netid;
            simState.motorState = cc.Motor.GetState();
            simState.forceMovementGravity = (physicsManager as FighterPhysicsManager3D).forceMovement;
            simState.forceMovementGravity.y = (physicsManager as FighterPhysicsManager3D).forceGravity.y;
            simState.mainState = (StateManager as FighterStateManager).CurrentState;
            simState.mainStateFrame = (StateManager as FighterStateManager).CurrentStateFrame;

            simState.currentJump = currentJump;
            simState.isGrounded = physicsManager.IsGrounded;
            simState.jumpHold = jumpHold;

            simState.lockedOn = LockedOn;
            simState.lockonForward = LockonForward;
            simState.lockOnTarget = LockonTarget;

            // Input Manager
            simState.inputBufferTick = inputManager.inputBufferTick;

            // Combat Manager
            simState.currentChargeLevel = (byte)combatManager.CurrentChargeLevel;
            simState.currentChargeLevelCharge = (ushort)combatManager.CurrentChargeLevelCharge;
            simState.hitstop = (ushort)combatManager.HitStop;
            simState.hitstun = (ushort)combatManager.HitStun;
            simState.currentMoveset = (byte)combatManager.CurrentMovesetIdentifier;
            simState.currentAttackMoveset = (sbyte)combatManager.CurrentAttackMovesetIdentifier;
            simState.currentAttackNode = (sbyte)combatManager.CurrentAttackNodeIdentifier;

            // Hitbox Manager
            simState.collidedIHurtables = hitboxManager.collidedIHurtables;

            // Hurtbox Manager
            simState.hurtboxHitCount = (hurtboxManager as FighterHurtboxManager).hurtboxHitCount;

            // State
            simState.stateSimState = (StateManager.GetState(StateManager.CurrentState) as FighterState).GetSimState();
        }

        public virtual void ApplySimState(ISimState state)
        {
            ObjectEnabled = state.objectEnabled;
            PlayerSimState pState = state as PlayerSimState;
            visual.transform.eulerAngles = new Vector3(0, pState.visualRotation, 0);
            cc.Motor.ApplyState(pState.motorState);
            physicsManager.SetGrounded(pState.isGrounded);
            currentJump = pState.currentJump;
            jumpHold = pState.jumpHold;

            (physicsManager as FighterPhysicsManager3D).forceGravity.y = pState.forceMovementGravity.y;
            pState.forceMovementGravity.y = 0;
            (physicsManager as FighterPhysicsManager3D).forceMovement = pState.forceMovementGravity;
            (StateManager as FighterStateManager).ChangeState(pState.mainState, pState.mainStateFrame);

            LockedOn = pState.lockedOn;
            LockonForward = pState.lockonForward;
            LockonTarget = pState.lockOnTarget;

            // Input Manager
            (inputManager as FighterInputManager).SetBufferTick(pState.inputBufferTick);

            // Combat Manager
            combatManager.SetChargeLevel(pState.currentChargeLevel);
            combatManager.SetChargeLevelCharge(pState.currentChargeLevelCharge);
            combatManager.SetHitStop(pState.hitstop);
            combatManager.SetHitStun(pState.hitstun);
            (combatManager as FighterCombatManager).ApplySimState(pState.currentMoveset, pState.currentAttackMoveset, pState.currentAttackNode);

            // Hitbox Manager
            hitboxManager.collidedIHurtables = pState.collidedIHurtables;

            // Hurtbox Manager
            (hurtboxManager as FighterHurtboxManager).hurtboxHitCount = pState.hurtboxHitCount;

            // State
            (StateManager.GetState(StateManager.CurrentState) as FighterState).ApplySimState(pState.stateSimState);
        }
    }
}