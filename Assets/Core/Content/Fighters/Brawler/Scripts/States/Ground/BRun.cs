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
            BrawlerManager bManager = Manager as BrawlerManager;
            FighterPhysicsManager physicsManager = Manager.PhysicsManager as FighterPhysicsManager;

            Vector3 translatedMovement = bManager.GetMovementVector();
            translatedMovement.y = 0;

            // Add velocity.
            Vector3 velo = (translatedMovement * bManager.stats.baseStats.runAcceleration)
                + (translatedMovement.normalized * bManager.stats.baseStats.runBaseAccel);
            physicsManager.forceMovement += velo;

            //Clamp movement velocity.
            if (physicsManager.forceMovement.magnitude > Stats.baseStats.maxRunSpeed)
            {
                physicsManager.forceMovement = physicsManager.forceMovement.normalized * Stats.baseStats.maxRunSpeed;
            }

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
            if (!Manager.IsGrounded)
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
 