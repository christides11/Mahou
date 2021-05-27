using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mahou.Content.Fighters
{
    public class FighterStateDash : FighterState
    {
        public override string GetName() => "Dash";

        Vector2 dir;

        public override void Initialize()
        {
            base.Initialize();
            Vector2 movementDir = InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT);
            if (movementDir.magnitude < InputConstants.movementThreshold)
            {
                movementDir = Vector2.up;
            }
            dir = movementDir.normalized;

            Vector3 mov = Manager.GetMovementVector(movementDir.x, movementDir.y);
            PhysicsManager.forceMovement = mov * Stats.CurrentStats.dashInitSpeed;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            FighterManager m = FighterManager;

            Vector3 mov = Manager.GetMovementVector(dir.x, dir.y);
            PhysicsManager.forceMovement += mov * Stats.CurrentStats.dashAcceleration;

            //Clamp movement velocity.
            if (PhysicsManager.forceMovement.magnitude > Stats.CurrentStats.maxDashSpeed)
            {
                PhysicsManager.forceMovement = PhysicsManager.forceMovement.normalized * Stats.CurrentStats.maxDashSpeed;
            }

            StateManager.IncrementFrame();
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryJump())
            {
                return true;
            }
            if (StateManager.CurrentStateFrame > Stats.CurrentStats.dashTime)
            {
                Vector2 movementDir = InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT);
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
    }
}
