using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BIdle : FighterState
    {
        public override void OnUpdate()
        {
            PhysicsManager.ApplyMovementFriction();
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            Manager.PhysicsManager.CheckIfGrounded();

            if ((Manager.InputManager as FighterInputManager).GetButton(Input.Action.Jump).firstPress)
            {
                StateManager.ChangeState((ushort)BrawlerState.JUMP_SQUAT);
                return true;
            }
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(Input.Action.Movement_X, 0);
            if (mov.magnitude > 0.2f)
            {
                StateManager.ChangeState((ushort)BrawlerState.WALK);
                return true;
            }
            return false;
        }
    }
}