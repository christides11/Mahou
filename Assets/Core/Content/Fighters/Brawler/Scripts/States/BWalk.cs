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
            BrawlerManager bManager = Manager as BrawlerManager;
            FighterPhysicsManager physicsManager = Manager.PhysicsManager as FighterPhysicsManager;

            Vector3 translatedMovement = bManager.GetMovementVector();
            translatedMovement.y = 0;

            // Add velocity.
            Vector3 velo = (translatedMovement * bManager.stats.walkAcceleration)
                + (translatedMovement.normalized * bManager.stats.walkBaseAccel);
            physicsManager.forceMovement += velo;

            //Clamp movement velocity.
            if (physicsManager.forceMovement.magnitude > bManager.stats.maxWalkSpeed)
            {
                physicsManager.forceMovement = physicsManager.forceMovement.normalized * bManager.stats.maxWalkSpeed;
            }

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if ((Manager.InputManager as FighterInputManager).GetButton(Input.Action.Jump).firstPress)
            {
                StateManager.ChangeState((ushort)BrawlerState.JUMP_SQUAT);
                return true;
            }
            Manager.PhysicsManager.CheckIfGrounded();
            if (!Manager.IsGrounded)
            {
                StateManager.ChangeState((ushort)BrawlerState.FALL);
                return true;
            }
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(Input.Action.Movement_X, 0);
            if(mov.magnitude < InputConstants.movementThreshold)
            {
                StateManager.ChangeState((ushort)BrawlerState.IDLE);
                return true;
            }
            if(InputManager.GetButton(Input.Action.Dash, 0).firstPress)
            {
                StateManager.ChangeState((ushort)BrawlerState.DASH);
                return true;
            }
            return false;
        }
    }
}