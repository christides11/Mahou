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
            if((controller as FighterManager).LockedOn == false && (controller as FighterManager).LockonTarget != null)
            {
                if(controller.InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT).magnitude >= InputConstants.movementThreshold)
                {
                    return AttackEventReturnType.NONE;
                }
                Vector3 dir = ((controller as FighterManager).LockonTarget.transform.position - controller.transform.position);
                dir.y = 0;
                dir.Normalize();

                controller.RotateVisual(dir, rotSpeed);
            }
            return AttackEventReturnType.NONE;
        }
    }
}