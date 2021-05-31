using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateWalk : FighterState
    {
        public override void OnUpdate()
        {
            FighterPhysicsManager physicsManager = FighterManager.PhysicsManager as FighterPhysicsManager;
            PhysicsManager.forceGravity = Vector3.zero;

            physicsManager.HandleMovement(FighterManager.StatsManager.CurrentStats.walkBaseAccel, FighterManager.StatsManager.CurrentStats.walkAcceleration,
                FighterManager.StatsManager.CurrentStats.groundFriction, FighterManager.StatsManager.CurrentStats.maxWalkSpeed, 
                FighterManager.StatsManager.CurrentStats.walkAccelFromDot);

            Vector3 movement = FighterManager.GetMovementVector();
            movement.y = 0;
            if (FighterManager.LockedOn) {
                FighterManager.RotateVisual(FighterManager.LockonForward, 10);
            }
            else {
                FighterManager.RotateVisual(movement.normalized, FighterManager.StatsManager.CurrentStats.walkRotationSpeed);
            }

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryAttack())
            {
                return true;
            }
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
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D((int)PlayerInputType.MOVEMENT, 0);
            if (mov.magnitude < InputConstants.movementThreshold)
            {
                StateManager.ChangeState((ushort)FighterStates.IDLE);
                return true;
            }
            if (InputManager.GetButton((int)PlayerInputType.DASH, 0).firstPress)
            {
                StateManager.ChangeState((ushort)FighterStates.DASH);
                return true;
            }
            return false;
        }
    }
}