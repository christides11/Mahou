using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BAirJump : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            Manager.IsGrounded = false;

            Vector3 mVector = (Manager as FighterManager).GetMovementVector();
            mVector.y = 0;
            PhysicsManager.forceMovement *= (Manager as FighterManager).StatsManager.baseStats.airJumpConversedMomentum;
            if (mVector.magnitude >= InputConstants.movementThreshold)
            {
                PhysicsManager.forceMovement += mVector * Stats.baseStats.airJumpHozVelo;
            }

            // Air jump force.
            PhysicsManager.forceGravity.y = (Manager as FighterManager).StatsManager.baseStats.airJumpVelocity[FighterManager.currentJump-2];
        }

        public override void OnUpdate()
        {
            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.ApplyMovement(es.baseStats.airAcceleration, es.baseStats.maxAirSpeed, es.baseStats.airDeceleration);
            PhysicsManager.HandleGravity();
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryJump())
            {
                return true;
            }
            if (InputManager.GetButton(Input.Action.Dash, 0).firstPress)
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
