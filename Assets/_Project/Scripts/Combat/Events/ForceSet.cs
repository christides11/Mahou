using HnSF.Combat;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.Events
{
    public class ForceSet : HnSF.Combat.AttackEvent
    {
        public bool applyXZForce;
        public bool applyYForce;

        public Vector2 xzForce;
        public float yForce;

        public override string GetName()
        {
            return "Set Forces";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterManager e = (FighterManager)controller;
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)controller.PhysicsManager;
            FighterInputManager inputManager = (FighterInputManager)controller.InputManager;
            Vector3 f = Vector3.zero;
            if (applyXZForce)
            {
                f.x = xzForce.x;
                f.z = xzForce.y;
            }
            if (applyYForce)
            {
                f.y = yForce;
            }

            if (applyYForce)
            {
                physicsManager.forceGravity.y = f.y;
            }
            if (applyXZForce)
            {
                physicsManager.forceMovement = (f.x * e.visual.transform.right) + (f.z * e.visual.transform.forward);
            }
            return AttackEventReturnType.NONE;
        }
    }
}