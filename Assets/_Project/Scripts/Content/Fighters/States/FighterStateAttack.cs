using Mahou.Combat;
using Mahou.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateAttack : FighterState
    {
        public class AttackSimStateVars : PlayerStateSimState
        {
            public Dictionary<int, bool> eventInputThing = new Dictionary<int, bool>();
        }

        public override string GetName()
        {
            return "Attack";
        }

        public override PlayerStateSimState GetSimState()
        {
            return new AttackSimStateVars()
            {
                eventInputThing = new Dictionary<int, bool>(eventInputThing)
            };
        }

        public override void ApplySimState(PlayerStateSimState simState)
        {
            if(simState == null)
            {
                return;
            }
            AttackSimStateVars assv = simState as AttackSimStateVars;
            if(assv == null)
            {
                return;
            }
            eventInputThing = new Dictionary<int, bool>(assv.eventInputThing);
        }

        public Dictionary<int, bool> eventInputThing = new Dictionary<int, bool>();
        public override void Initialize()
        {
            eventInputThing.Clear();
            (Manager as FighterManager).charging = true;
            (Manager.HurtboxManager as FighterHurtboxManager).Reset();
            AttackDefinition currentAttack = (AttackDefinition)FighterManager.CombatManager.CurrentAttackNode.attackDefinition;
            if (currentAttack.useState)
            {
                FighterManager.StateManager.ChangeState(currentAttack.stateOverride);
                return;
            }
        }

        bool eventCancel;
        public override void OnUpdate()
        {
            FighterManager entityManager = FighterManager;
            AttackDefinition currentAttack = (AttackDefinition)entityManager.CombatManager.CurrentAttackNode.attackDefinition;
            if (currentAttack.hurtboxDefinition)
            {
                (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                    currentAttack.hurtboxDefinition,
                    StateManager.CurrentStateFrame);
            }
            
            if (TryCancelWindow(currentAttack))
            {
                return;
            }

            eventCancel = false;
            bool interrupted = false;
            bool cleanup = false;
            for (int i = 0; i < currentAttack.events.Count; i++)
            {
                if (currentAttack.events[i].CheckConditions(Manager) == false)
                {
                    continue;
                }
                switch (HandleEvents(i, currentAttack, currentAttack.events[i]))
                {
                    case HnSF.Combat.AttackEventReturnType.STALL:
                        // Event wants us to stall on the current frame.
                        eventCancel = true;
                        break;
                    case HnSF.Combat.AttackEventReturnType.INTERRUPT:
                        interrupted = true;
                        cleanup = true;
                        break;
                    case HnSF.Combat.AttackEventReturnType.INTERRUPT_NO_CLEANUP:
                        interrupted = true;
                        break;
                }
                if(interrupted == true)
                {
                    break;
                }
            }

            if (interrupted)
            {
                if (cleanup)
                {
                    entityManager.CombatManager.Cleanup();
                }
                return;
            }
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            AttackDefinition currentAttack = (AttackDefinition)FighterManager.CombatManager.CurrentAttackNode.attackDefinition;

            for (int i = 0; i < currentAttack.hitboxGroups.Count; i++)
            {
                HandleHitboxGroup(i, currentAttack.hitboxGroups[i]);
            }

            if (CheckInterrupt())
            {
                return;
            }

            FighterManager entityManager = FighterManager;
            if (eventCancel == false && HandleChargeLevels(entityManager, currentAttack) == false)
            {
                entityManager.StateManager.IncrementFrame();
            }
        }

        protected virtual bool TryCancelWindow(AttackDefinition currentAttack)
        {
            FighterManager e = FighterManager;
            for (int i = 0; i < currentAttack.cancels.Count; i++)
            {
                if (e.StateManager.CurrentStateFrame >= currentAttack.cancels[i].startFrame
                    && e.StateManager.CurrentStateFrame <= currentAttack.cancels[i].endFrame)
                {
                    int man = e.CombatManager.TryCancelList(currentAttack.cancels[i].cancelListID);
                    if (man != -1)
                    {
                        e.CombatManager.SetAttack(man);
                        e.StateManager.ChangeState((int)FighterStates.ATTACK);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handles processing the charge levels of the current attack.
        /// </summary>
        /// <param name="entityManager">The entity itself.</param>
        /// <param name="currentAttack">The current attack the entity is doing.</param>
        /// <returns>If the frame should be held.</returns>
        private bool HandleChargeLevels(FighterManager entityManager, AttackDefinition currentAttack)
        {
            FighterCombatManager cManager = (FighterCombatManager)entityManager.CombatManager;

            if(entityManager.charging == false)
            {
                return false;
            }

            bool result = false;
            for (int i = 0; i < currentAttack.chargeWindows.Count; i++)
            {
                if (entityManager.StateManager.CurrentStateFrame != currentAttack.chargeWindows[i].startFrame)
                {
                    continue;
                }

                PlayerInputType button = (currentAttack.chargeWindows[i] as Mahou.Combat.ChargeDefinition).input;
                if (entityManager.InputManager.GetButton((int)button).isDown == false)
                {
                    entityManager.charging = false;
                    result = false;
                    break;
                }

                // Still have charge levels to go through.
                if (entityManager.CombatManager.CurrentChargeLevel < currentAttack.chargeWindows[i].chargeLevels.Count)
                {
                    cManager.IncrementChargeLevelCharge(currentAttack.chargeWindows[i].chargeLevels[cManager.CurrentChargeLevel].maxChargeFrames);
                    // Charge completed, move on to the next level.
                    if (cManager.CurrentChargeLevelCharge == currentAttack.chargeWindows[i].chargeLevels[cManager.CurrentChargeLevel].maxChargeFrames)
                    {
                        cManager.SetChargeLevel(cManager.CurrentChargeLevel + 1);
                        cManager.SetChargeLevelCharge(0);
                    }
                }
                else if (currentAttack.chargeWindows[i].releaseOnCompletion)
                {
                    entityManager.charging = false;
                }
                result = true;
                // Only one charge level can be handled per frame, ignore everything else.
                break;
            }
            return result;
        }

        /// <summary>
        /// Handles the lifetime of events.
        /// </summary>
        /// <param name="currentEvent">The event being processed.</param>
        /// <returns>True if the current attack state was canceled by the event.</returns>
        protected virtual HnSF.Combat.AttackEventReturnType HandleEvents(int eventIndex, AttackDefinition currentAttack, HnSF.Combat.AttackEventDefinition currentEvent)
        {
            if (!currentEvent.active)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }
            FighterManager e = FighterManager;

            if(e.CombatManager.CurrentChargeLevel < currentEvent.chargeLevelMin
                || e.CombatManager.CurrentChargeLevel > currentEvent.chargeLevelMax)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }

            if (currentEvent.inputCheckTiming != HnSF.Combat.AttackEventInputCheckTiming.NONE
                && eventInputThing.ContainsKey(eventIndex) == false)
            {
                eventInputThing.Add(eventIndex, false);
            }

            // Input Checking.
            if (e.StateManager.CurrentStateFrame >= currentEvent.inputCheckStartFrame
                && e.StateManager.CurrentStateFrame <= currentEvent.inputCheckEndFrame)
            {
                switch (currentEvent.inputCheckTiming)
                {
                    case HnSF.Combat.AttackEventInputCheckTiming.ONCE:
                        if (currentEvent.inputCheckProcessed)
                        {
                            break;
                        }
                        eventInputThing[eventIndex] = e.CombatManager.CheckForInputSequence(currentEvent.input);
                        break;
                    case HnSF.Combat.AttackEventInputCheckTiming.CONTINUOUS:
                        eventInputThing[eventIndex] = e.CombatManager.CheckForInputSequence(currentEvent.input, 0, true, true);
                        break;
                }
            }

            if (currentEvent.inputCheckTiming != HnSF.Combat.AttackEventInputCheckTiming.NONE
                && eventInputThing[eventIndex] == false)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }

            if (e.StateManager.CurrentStateFrame >= currentEvent.startFrame
                && e.StateManager.CurrentStateFrame <= currentEvent.endFrame)
            {
                // Hit Check.
                if (currentEvent.onHitCheck != HnSF.Combat.OnHitType.NONE)
                {
                    if (currentEvent.onHitCheck == HnSF.Combat.OnHitType.ID_GROUP)
                    {
                        if (((FighterHitboxManager)e.CombatManager.hitboxManager).IDGroupHasHurt(currentEvent.onHitIDGroup) == false)
                        {
                            return HnSF.Combat.AttackEventReturnType.NONE;
                        }
                    }
                    else if (currentEvent.onHitCheck == HnSF.Combat.OnHitType.HITBOX_GROUP)
                    {
                        if (((FighterHitboxManager)e.CombatManager.hitboxManager).HitboxGroupHasHurt(currentAttack.hitboxGroups[currentEvent.onHitHitboxGroup].ID,
                            currentEvent.onHitHitboxGroup) == false)
                        {
                            return HnSF.Combat.AttackEventReturnType.NONE;
                        }
                    }
                }
                return currentEvent.attackEvent.Evaluate((int)(e.StateManager.CurrentStateFrame - currentEvent.startFrame),
                    currentEvent.endFrame - currentEvent.startFrame,
                    e,
                    currentEvent.variables);
            }
            return HnSF.Combat.AttackEventReturnType.NONE;
        }

        /// <summary>
        /// Handles the lifetime process of box groups.
        /// </summary>
        /// <param name="groupIndex">The group number being processed.</param>
        /// <param name="boxGroup">The group being processed.</param>
        protected virtual void HandleHitboxGroup(int groupIndex, HnSF.Combat.HitboxGroup boxGroup)
        {
            FighterManager entityManager = FighterManager;
            // Make sure we're in the frame window of the box.
            if (entityManager.StateManager.CurrentStateFrame < boxGroup.activeFramesStart
                || entityManager.StateManager.CurrentStateFrame > boxGroup.activeFramesEnd)
            {
                return;
            }

            // Check if the charge level requirement was met.
            if (boxGroup.chargeLevelNeeded >= 0)
            {
                int currentChargeLevel = entityManager.CombatManager.CurrentChargeLevel;
                if (currentChargeLevel < boxGroup.chargeLevelNeeded
                    || currentChargeLevel > boxGroup.chargeLevelMax)
                {
                    return;
                }
            }

            // Hit check.
            switch (boxGroup.hitboxHitInfo.hitType)
            {
                case HnSF.Combat.HitboxType.HIT:
                    bool hitResult = entityManager.CombatManager.hitboxManager.CheckForCollision(groupIndex, boxGroup, Manager.visual);
                    if(hitResult == true)
                    {
                        if ((boxGroup.hitboxHitInfo as HitInfo).hitSound != null)
                        {
                            SimulationAudioManager.Play((boxGroup.hitboxHitInfo as HitInfo).hitSound, Manager.transform.position, AudioPlayMode.ROLLBACK);
                        }
                    }
                    break;
            }
        }

        public override bool CheckInterrupt()
        {
            FighterManager entityManager = FighterManager;
            if (entityManager.TryAttack())
            {
                return true;
            }
            if(FirstInterruptableFrameCheck())
            {
                Debug.Log("Interrupt frame.");
                Manager.CombatManager.Cleanup();
                return true;
            }
            if (entityManager.StateManager.CurrentStateFrame >
                entityManager.CombatManager.CurrentAttackNode.attackDefinition.length)
            {
                if (entityManager.PhysicsManager.IsGrounded)
                {
                    entityManager.StateManager.ChangeState((int)FighterStates.IDLE);
                }
                else
                {
                    entityManager.StateManager.ChangeState((int)FighterStates.FALL);
                }
                Manager.CombatManager.Cleanup();
                return true;
            }
            return false;
        }

        private bool FirstInterruptableFrameCheck()
        {
            FighterManager entityManager = FighterManager;
            AttackDefinition currentAttack = (AttackDefinition)FighterManager.CombatManager.CurrentAttackNode.attackDefinition;
            if (entityManager.StateManager.CurrentStateFrame < currentAttack.firstActableFrame)
            {
                return false;
            }

            if (entityManager.TryJump())
            {
                return true;
            }
            Vector2 move = entityManager.InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT);
            if(move.magnitude >= InputConstants.movementThreshold)
            {
                entityManager.StateManager.ChangeState((int)FighterStates.WALK);
                return true;
            }
            return false;
        }
    }
}