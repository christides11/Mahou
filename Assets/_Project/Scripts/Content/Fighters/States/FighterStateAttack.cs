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
                //HandleHitboxGroup(i, currentAttack.hitboxGroups[i]);
            }

            /*
            if (TryCancelWindow(currentAttack))
            {
                return;
            }*/

            bool eventCancel = false;
            bool interrupted = false;
            bool cleanup = false;
            for (int i = 0; i < currentAttack.events.Count; i++)
            {
                /*
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
                }*/
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

        public override bool CheckInterrupt()
        {
            return base.CheckInterrupt();
        }
    }
}