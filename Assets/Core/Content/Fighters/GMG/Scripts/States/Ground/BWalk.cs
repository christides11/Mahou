using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BWalk : FighterState
    {
        public override void OnUpdate()
        {
            GMGManager m = Manager as GMGManager;
            FighterPhysicsManager physicsManager = m.PhysicsManager as FighterPhysicsManager;
            PhysicsManager.forceGravity = Vector3.zero;

            physicsManager.HandleMovement(m.StatsManager.CurrentStats.walkBaseAccel, m.StatsManager.CurrentStats.walkAcceleration,
                m.StatsManager.CurrentStats.groundFriction, m.StatsManager.CurrentStats.maxWalkSpeed, m.StatsManager.CurrentStats.walkAccelFromDot);

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryJump())
            {
                return true;
            }
            Manager.PhysicsManager.CheckIfGrounded();
            if (!Manager.PhysicsManager.IsGrounded)
            {
                StateManager.ChangeState((ushort)FighterStates.FALL);
                return true;
            }
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(Input.Action.Movement_X, 0);
            if(mov.magnitude < InputConstants.movementThreshold)
            {
                StateManager.ChangeState((ushort)FighterStates.IDLE);
                return true;
            }
            if(InputManager.GetButton(Input.Action.Dash, 0).firstPress)
            {
                StateManager.ChangeState((ushort)FighterStates.DASH);
                return true;
            }
            return false;
        }
    }
}