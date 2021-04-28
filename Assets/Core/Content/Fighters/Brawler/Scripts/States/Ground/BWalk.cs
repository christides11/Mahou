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
            Vector3 velo = (translatedMovement * bManager.stats.baseStats.walkAcceleration)
                + (translatedMovement.normalized * bManager.stats.baseStats.walkBaseAccel);
            physicsManager.forceMovement += velo;

            //Clamp movement velocity.
            if (physicsManager.forceMovement.magnitude > bManager.stats.baseStats.maxWalkSpeed)
            {
                physicsManager.forceMovement = physicsManager.forceMovement.normalized * bManager.stats.baseStats.maxWalkSpeed;
            }

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryJump())
            {
                return true;
            }
            Manager.PhysicsManager.CheckIfGrounded();
            if (!Manager.IsGrounded)
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