using UnityEngine;
using System.Collections.Generic;
using HnSF.Combat;
using Mahou;
using Mahou.Content.Fighters;

namespace TDAction.Combat.Events
{
    public class LandCancel : AttackEvent
    {
        public enum CancelType
        {
            DEFAULT = 0,
            STATE = 1,
            ATTACK = 2
        }

        public CancelType cancelType;
        public FighterStates state;
        public int movesetIdentifier = -1;
        public int attackIdentifier;

        public override string GetName()
        {
            return "Land Cancel";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            /*
            if ((controller as FighterManager).TryLandCancel(cancelType == CancelType.DEFAULT ? true : false))
            {
                if (cancelType == CancelType.STATE)
                {
                    controller.StateManager.ChangeState((ushort)state);
                    return AttackEventReturnType.INTERRUPT;
                }
                else if (cancelType == CancelType.ATTACK)
                {
                    controller.CombatManager.Cleanup();
                    (controller as TDAction.Fighter.FighterManager).TryAttack(attackIdentifier, movesetIdentifier);
                    return AttackEventReturnType.INTERRUPT_NO_CLEANUP;
                }
                return AttackEventReturnType.INTERRUPT;
            }*/
            return AttackEventReturnType.NONE;
        }
    }
}