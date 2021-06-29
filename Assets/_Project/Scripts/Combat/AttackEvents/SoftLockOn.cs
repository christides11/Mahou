using UnityEngine;
using System.Collections.Generic;
using HnSF.Combat;
using Mahou;
using Mahou.Content.Fighters;

namespace Mahou.Combat.AttackEvents
{
    public class SoftLockOn : AttackEvent
    {
        public override string GetName()
        {
            return "Soft Lock On";
        }

        public float rotSpeed = 10;

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            (controller as FighterManager).PickSoftlockTarget();
            Vector2 move = controller.InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT);
            Vector3 dir = Vector3.zero;

            if (move.magnitude >= InputConstants.movementThreshold)
            {
                dir = (controller as FighterManager).GetMovementVector(move.x, move.y);
                dir.Normalize();
                controller.RotateVisual(dir, rotSpeed);
                return AttackEventReturnType.NONE;
            }

            if((controller as FighterManager).LockonTarget != null)
            {
                dir = ((controller as FighterManager).LockonTarget.transform.position - controller.transform.position);
                dir.y = 0;
                dir.Normalize();
                controller.RotateVisual(dir, rotSpeed);
            }

            return AttackEventReturnType.NONE;
        }
    }
}