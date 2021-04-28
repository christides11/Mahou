using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BJump : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            Manager.IsGrounded = false;

            Vector3 mVector = (Manager as FighterManager).GetMovementVector();
            mVector.y = 0;
            PhysicsManager.forceMovement *= (Manager as FighterManager).StatsManager.baseStats.jumpConversedMomentum;
            if(mVector.magnitude >= InputConstants.movementThreshold)
            {
                PhysicsManager.forceMovement += mVector * Stats.baseStats.jumpInitHozVelo;
            }

            // Transfer moving platform forces into actual force.
            Vector3 tempPhysicsMover = (Manager as FighterManager).cc.Motor.AttachedRigidbodyVelocity;
            PhysicsManager.forceGravity.y = tempPhysicsMover.y;
            tempPhysicsMover.y = 0;
            PhysicsManager.forceMovement += tempPhysicsMover;

            // Ignore negative gravity.
            if (PhysicsManager.forceGravity.y < 0)
            {
                PhysicsManager.forceGravity.y = 0;
            }

            // Add jump force.
            PhysicsManager.forceGravity.y += (Manager as FighterManager).fullHop ? (Manager as FighterManager).StatsManager.baseStats.fullHopVelocity
                : (Manager as FighterManager).StatsManager.baseStats.shortHopVelocity;
        }

        public override void OnUpdate()
        {
            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.ApplyMovement(es.baseStats.airAcceleration, es.baseStats.maxAirSpeed, es.baseStats.airDeceleration);
            PhysicsManager.HandleGravity();
            /*if (controller.LockedOn)
            {
                controller.RotateVisual(controller.LockonForward, es.airRotationSpeed);
            }
            else
            {
                controller.RotateVisual(controller.GetMovementVector(0), es.airRotationSpeed);
            }*/
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
