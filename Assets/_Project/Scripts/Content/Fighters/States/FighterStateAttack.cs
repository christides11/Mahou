using Mahou.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateAttack : FighterState
    {
        public override string GetName()
        {
            return "Attack";
        }

        public override void Initialize()
        {
            AttackDefinition currentAttack = (AttackDefinition)FighterManager.CombatManager.CurrentAttackNode.attackDefinition;
            if (currentAttack.useState)
            {
                FighterManager.StateManager.ChangeState(currentAttack.stateOverride);
                return;
            }
            //charging = true;
            /*
            if (String.IsNullOrEmpty(currentAttack.animationName) == false)
            {
                (Manager as FighterManager).entityAnimator.PlayAnimation((Manager as FighterManager).GetAnimationClip(currentAttack.animationName,
                    GetEntityManager().CombatManager.CurrentAttackMovesetIdentifier));
            }*/
        }

        public override void OnUpdate()
        {
            FighterManager entityManager = FighterManager;
            AttackDefinition currentAttack = (AttackDefinition)entityManager.CombatManager.CurrentAttackNode.attackDefinition;

            for (int i = 0; i < currentAttack.hitboxGroups.Count; i++)
            {
                HandleHitboxGroup(i, currentAttack.hitboxGroups[i]);
            }

            
            if (TryCancelWindow(currentAttack))
            {
                return;
            }

            bool eventCancel = false;
            bool interrupted = false;
            bool cleanup = false;
            for (int i = 0; i < currentAttack.events.Count; i++)
            {
                switch (HandleEvents(currentAttack, currentAttack.events[i]))
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
            }

            if (interrupted)
            {
                if (cleanup)
                {
                    entityManager.CombatManager.Cleanup();
                }
                return;
            }
            if (CheckInterrupt())
            {
                return;
            }

            //if (!eventCancel && !HandleChargeLevels(entityManager, currentAttack))
            if(!eventCancel)
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
        /// Handles the lifetime of events.
        /// </summary>
        /// <param name="currentEvent">The event being processed.</param>
        /// <returns>True if the current attack state was canceled by the event.</returns>
        protected virtual HnSF.Combat.AttackEventReturnType HandleEvents(AttackDefinition currentAttack, HnSF.Combat.AttackEventDefinition currentEvent)
        {
            if (!currentEvent.active)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }
            FighterManager e = FighterManager;

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
                        currentEvent.inputCheckProcessed = e.CombatManager.CheckForInputSequence(currentEvent.input);
                        Debug.Log($"?{currentEvent.inputCheckProcessed}, ({currentEvent.input.executeInputs.Count})");
                        break;
                    case HnSF.Combat.AttackEventInputCheckTiming.CONTINUOUS:
                        currentEvent.inputCheckProcessed = e.CombatManager.CheckForInputSequence(currentEvent.input, 0, true, true);
                        break;
                }
            }

            if (currentEvent.inputCheckTiming != HnSF.Combat.AttackEventInputCheckTiming.NONE
                && !currentEvent.inputCheckProcessed)
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
                    entityManager.CombatManager.hitboxManager.CheckForCollision(groupIndex, boxGroup);
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
    }
}