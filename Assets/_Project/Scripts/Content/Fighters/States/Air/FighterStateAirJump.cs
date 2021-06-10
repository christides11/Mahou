using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateAirJump : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            Manager.PhysicsManager.SetGrounded(false);

            Vector3 mVector = (Manager as FighterManager).GetMovementVector();
            mVector.y = 0;
            PhysicsManager.forceMovement *= (Manager as FighterManager).StatsManager.CurrentStats.airJumpConversedMomentum;
            if (mVector.magnitude >= InputConstants.movementThreshold)
            {
                PhysicsManager.forceMovement += mVector * Stats.CurrentStats.airJumpHozVelo;
            }

            // Air jump force.
            PhysicsManager.forceGravity.y = (Manager as FighterManager).StatsManager.CurrentStats.airJumpVelocity[FighterManager.currentJump - 2].GetCurrentValue();
        }

        public override void OnUpdate()
        {
            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.HandleMovement(es.CurrentStats.airBaseAccel, es.CurrentStats.airAccel, es.CurrentStats.airDeceleration,
                es.CurrentStats.maxAirSpeed, es.CurrentStats.accelFromDotProduct);
            PhysicsManager.HandleGravity();
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
            if (InputManager.GetButton((int)PlayerInputType.DASH, 0).firstPress)
            {
                StateManager.ChangeState((ushort)FighterStates.AIR_DASH);
                return true;
            }
            if (PhysicsManager.forceGravity.y <= 0)
            {
                StateManager.ChangeState((int)FighterStates.FALL);
                return true;
            }
            return false;
        }
    }
}