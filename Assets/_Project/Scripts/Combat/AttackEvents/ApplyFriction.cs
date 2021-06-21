using HnSF.Combat;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.AttackEvents
{
    public class ApplyFriction : HnSF.Combat.AttackEvent
    {
        public bool useXZFriction;
        public bool useYFriction;

        public float xzFriction;
        public float yFriction;

        public override string GetName()
        {
            return "Friction";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)controller.PhysicsManager;
            if (useXZFriction)
            {
                physicsManager.ApplyMovementFriction(xzFriction);
            }
            if (useYFriction)
            {
                physicsManager.ApplyGravityFriction(yFriction);
            }
            return AttackEventReturnType.NONE;
        }
    }
}