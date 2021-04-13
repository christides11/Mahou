using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BJumpSquat : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            (Manager as FighterManager).fullHop = true;
            PhysicsManager.ApplyMovementFriction();
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
            if (Manager.InputManager.GetButton((int)Mahou.Input.Action.Jump).released)
            {
                (Manager as FighterManager).fullHop = false;
            }
            CheckInterrupt();
            Manager.StateManager.IncrementFrame();
        }

        public override bool CheckInterrupt()
        {
            if (Manager.StateManager.CurrentStateFrame >= (Manager as FighterManager).Stats.jumpSquat)
            {
                Manager.StateManager.ChangeState((int)BrawlerState.JUMP);
                return true;
            }
            return false;
        }
    }
}