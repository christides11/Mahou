using HnSF.Combat;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.AttackEvents
{
    public class ForceAdd : HnSF.Combat.AttackEvent
    {
        public bool addXZForce;
        public bool addYForce;

        public Vector2 xzForce;
        public float yForce;

        public override string GetName()
        {
            return "Add Forces";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterManager e = (FighterManager)controller;
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)controller.PhysicsManager;
            FighterInputManager inputManager = (FighterInputManager)controller.InputManager;
            Vector3 f = Vector3.zero;
            if (addXZForce)
            {
                f.x = xzForce.x;
                f.z = xzForce.y;
            }
            if (addYForce)
            {
                f.y = yForce;
            }

            if (addYForce)
            {
                physicsManager.forceGravity.y += f.y;
            }
            if (addXZForce)
            {
                physicsManager.forceMovement += (f.x * e.visual.transform.right) + (f.z * e.visual.transform.forward);
            }
            return AttackEventReturnType.NONE;
        }
    }
}