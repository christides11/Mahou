using UnityEngine;
using System.Collections.Generic;
using HnSF.Combat;
using Mahou;
using Mahou.Content.Fighters;

namespace Mahou.Combat.AttackEvents
{
    public class JumpCancel : AttackEvent
    {
        public override string GetName()
        {
            return "Jump Cancel";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            if((controller as FighterManager).TryJump())
            {
                return AttackEventReturnType.INTERRUPT;
            }
            return AttackEventReturnType.NONE;
        }
    }
}