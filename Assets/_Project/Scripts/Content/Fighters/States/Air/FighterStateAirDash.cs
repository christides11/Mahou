using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateAirDash : FighterState
    {
        public override void Initialize()
        {
            Vector3 translatedMovement = FighterManager.GetMovementVector();
            translatedMovement.y = 0;
            if (translatedMovement.magnitude < InputConstants.movementThreshold)
            {
                translatedMovement = Manager.GetMovementVector(0, 1);
            }

            PhysicsManager.forceGravity = Vector3.zero;
            PhysicsManager.forceMovement = translatedMovement.normalized * Stats.CurrentStats.airDashVelocityCurve.Evaluate(0);
        }

        public override void OnUpdate()
        {
            PhysicsManager.forceMovement = PhysicsManager.forceMovement.normalized
                * Stats.CurrentStats.airDashVelocityCurve.Evaluate((float)StateManager.CurrentStateFrame / (float)Stats.CurrentStats.airDashLength);

            if (StateManager.CurrentStateFrame >= Stats.CurrentStats.airDashGravityAfter)
            {
                PhysicsManager.HandleGravity();
            }

            CheckInterrupt();
            StateManager.IncrementFrame();
        }

        public override bool CheckInterrupt()
        {
            if (StateManager.CurrentStateFrame >= Stats.CurrentStats.airDashLength)
            {
                StateManager.ChangeState((ushort)FighterStates.FALL);
                return true;
            }

            if (Manager.PhysicsManager.IsGrounded)
            {
                StateManager.ChangeState((ushort)FighterStates.IDLE);
                return true;
            }
            return false;
        }
    }
}