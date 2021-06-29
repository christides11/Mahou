using HnSF.Combat;
using HnSF.Fighters;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.AttackEvents
{
    public class ClampGravity : AttackEvent
    {
        public float maxLength;

        public override string GetName()
        {
            return "Clamp Gravity";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)controller.PhysicsManager;
            if (maxLength == 0)
            {
                physicsManager.forceGravity = Vector3.zero;
            }
            else
            {
                physicsManager.forceGravity = Vector3.ClampMagnitude(physicsManager.forceGravity, maxLength);
            }
            return AttackEventReturnType.NONE;
        }
    }
}