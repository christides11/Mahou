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
            if (mVector.magnitude >= InputConstants.movementThreshold)
            {
                PhysicsManager.forceMovement = mVector;
                PhysicsManager.forceMovement *= (Manager as FighterManager).Stats.jumpConversedMomentum;
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
            PhysicsManager.forceGravity.y += (Manager as FighterManager).fullHop ? (Manager as FighterManager).Stats.fullHopVelocity
                : (Manager as FighterManager).Stats.shortHopVelocity;
        }

        public override void OnUpdate()
        {
            FighterStats es = (Manager as FighterManager).Stats;
            PhysicsManager.ApplyMovement(es.airAcceleration, es.maxAirSpeed, es.airDeceleration);
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
            if (PhysicsManager.forceGravity.y <= 0)
            {
                StateManager.ChangeState((int)BrawlerState.FALL);
                return true;
            }
            return false;
        }
    }
}
