using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BRun : FighterState
    {
        public override void OnUpdate()
        {
            GMGManager m = Manager as GMGManager;
            FighterPhysicsManager physicsManager = Manager.PhysicsManager as FighterPhysicsManager;

            physicsManager.HandleMovement(m.StatsManager.CurrentStats.runBaseAccel, m.StatsManager.CurrentStats.runAcceleration,
                m.StatsManager.CurrentStats.groundFriction, m.StatsManager.CurrentStats.maxRunSpeed, m.StatsManager.CurrentStats.runAccelFromDot);

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if ((Manager.InputManager as FighterInputManager).GetButton(Input.Action.Jump).firstPress)
            {
                StateManager.ChangeState((ushort)FighterStates.JUMP_SQUAT);
                return true;
            }
            Manager.PhysicsManager.CheckIfGrounded();
            if (!Manager.PhysicsManager.IsGrounded)
            {
                StateManager.ChangeState((ushort)FighterStates.FALL);
                return true;
            }
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(0, 0);
            if (mov.magnitude <= 0.2f)
            {
                StateManager.ChangeState((ushort)FighterStates.IDLE);
                return true;
            }
            return false;
        }

        public override string GetName() => "Run";
    }
}
 