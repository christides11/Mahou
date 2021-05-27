using HnSF.Combat;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.Events
{
    public class ClampMovement : HnSF.Combat.AttackEvent
    {
        public float maxLength;

        public override string GetName()
        {
            return "Clamp Movement";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)controller.PhysicsManager;
            physicsManager.forceMovement = Vector3.ClampMagnitude(physicsManager.forceMovement, maxLength);
            return AttackEventReturnType.NONE;
        }
    }
}
