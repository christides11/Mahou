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

            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(0, 0);
            Vector3 translatedMovement = Manager.GetMovementVector(mov.x, mov.y);
            translatedMovement.y = 0;

            // Add velocity.
            Vector3 velo = (translatedMovement * bManager.stats.walkAcceleration)
                + (translatedMovement.normalized * bManager.stats.walkBaseAccel);
            physicsManager.forceMovement += velo;

            //Clamp movement velocity.
            if (physicsManager.forceMovement.magnitude >
                bManager.stats.maxWalkSpeed * translatedMovement.magnitude)
            {
                physicsManager.forceMovement = physicsManager.forceMovement.normalized
                    * bManager.stats.maxWalkSpeed * translatedMovement.magnitude;
            }

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(0, 0);
            if(mov.magnitude <= 0.2f)
            {
                StateManager.ChangeState((ushort)BrawlerState.IDLE);
                return true;
            }
            return false;
        }
    }
}