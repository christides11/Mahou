using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BDash : FighterState
    {
        Vector2 dir;

        public override void Initialize()
        {
            base.Initialize();
            Vector2 movementDir = InputManager.GetAxis2D(Mahou.Input.Action.Movement_X);
            if(movementDir.magnitude < InputConstants.movementThreshold)
            {
                movementDir = Vector2.up;
            }
            dir = movementDir.normalized;

            Vector3 mov = Manager.GetMovementVector(movementDir.x, movementDir.y);
            PhysicsManager.forceMovement = mov * Stats.baseStats.dashInitSpeed;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            Vector3 mov = Manager.GetMovementVector(dir.x, dir.y);
            PhysicsManager.forceMovement += mov * Stats.baseStats.dashAcceleration;

            //Clamp movement velocity.
            if (PhysicsManager.forceMovement.magnitude > Stats.baseStats.maxDashSpeed)
            {
                PhysicsManager.forceMovement = PhysicsManager.forceMovement.normalized * Stats.baseStats.maxDashSpeed;
            }

            StateManager.IncrementFrame();
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (StateManager.CurrentStateFrame > Stats.baseStats.dashTime)
            {
                Vector2 movementDir = InputManager.GetAxis2D(Mahou.Input.Action.Movement_X);
                if (movementDir.magnitude < InputConstants.movementThreshold)
                {
                    StateManager.ChangeState((ushort)FighterStates.IDLE);
                    return true;
                }
                else
                {
                    StateManager.ChangeState((ushort)FighterStates.RUN);
                    return true;
                }
            }
            return false;
        }

        public override string GetName() => "Dash";
    }
}