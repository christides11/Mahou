using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateJumpSquat : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            PhysicsManager.ApplyMovementFriction((Manager as FighterManager).StatsManager.CurrentStats.jumpSquatFriction);
            /*if (controller.LockedOn)
            {
                controller.SetVisualRotation(controller.LockonForward);
            }
            else
            {
            Vector3 lookVector = (Manager as FighterManager).GetMovementVector();
            if (lookVector.magnitude >= InputConstants.movementThreshold)
            {
                Manager.SetVisualRotation(lookVector);
            }*/
        }

        public override void OnUpdate()
        {
            PhysicsManager.ApplyMovementFriction((Manager as FighterManager).StatsManager.CurrentStats.jumpSquatFriction);

            if (CheckInterrupt() == false)
            {
                Manager.StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()
        {
            if (Manager.StateManager.CurrentStateFrame >= (Manager as FighterManager).StatsManager.CurrentStats.jumpSquat)
            {
                Manager.StateManager.ChangeState((int)FighterStates.JUMP);
                return true;
            }
            return false;
        }
    }
}