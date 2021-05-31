using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateRun : FighterState
    {
        public override string GetName() => "Run";

        public override void OnUpdate()
        {
            FighterManager m = FighterManager;
            FighterPhysicsManager physicsManager = Manager.PhysicsManager as FighterPhysicsManager;

            physicsManager.HandleMovement(m.StatsManager.CurrentStats.runBaseAccel, m.StatsManager.CurrentStats.runAcceleration,
                m.StatsManager.CurrentStats.groundFriction, m.StatsManager.CurrentStats.maxRunSpeed, m.StatsManager.CurrentStats.runAccelFromDot);

            Vector3 movement = FighterManager.GetMovementVector();
            movement.y = 0;
            FighterManager.RotateVisual(movement.normalized, FighterManager.StatsManager.CurrentStats.runRotationSpeed);

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryAttack())
            {
                return true;
            }
            if ((Manager.InputManager as FighterInputManager).GetButton((int)PlayerInputType.JUMP).firstPress)
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
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D((int)PlayerInputType.MOVEMENT, 0);
            if (mov.magnitude < InputConstants.movementThreshold)
            {
                StateManager.ChangeState((ushort)FighterStates.IDLE);
                return true;
            }
            return false;
        }
    }
}