using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateJump : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            Manager.PhysicsManager.SetGrounded(false);

            Vector3 mVector = (Manager as FighterManager).GetMovementVector();
            mVector.y = 0;
            PhysicsManager.forceMovement *= (Manager as FighterManager).StatsManager.CurrentStats.jumpConversedMomentum;
            if (mVector.magnitude >= InputConstants.movementThreshold)
            {
                PhysicsManager.forceMovement += mVector * Stats.CurrentStats.jumpInitHozVelo;
            }

            // Transfer moving platform forces into actual force.
            Vector3 tempPhysicsMover = (Manager as FighterManager).cc.Motor.AttachedRigidbodyVelocity;
            PhysicsManager.forceGravity.y = tempPhysicsMover.y;
            tempPhysicsMover.y = 0;
            PhysicsManager.forceMovement += tempPhysicsMover;

            // Ignore negative gravity.
            if (PhysicsManager.forceGravity.y <= 0)
            {
                PhysicsManager.forceGravity.y = 0;
            }

            // Add jump force.
            PhysicsManager.forceGravity.y = (Manager as FighterManager).StatsManager.CurrentStats.jumpVelocity;
            (Manager as FighterManager).jumpHold = true;
        }

        public override void OnUpdate()
        {
            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.HandleMovement(es.CurrentStats.airBaseAccel, es.CurrentStats.airAccel, es.CurrentStats.airDeceleration,
                es.CurrentStats.maxAirSpeed, es.CurrentStats.accelFromDotProduct);

            if (StateManager.CurrentStateFrame > es.CurrentStats.jumpVeloMaxHoldFrames || (Manager as FighterManager).jumpHold == false)
            {
                PhysicsManager.HandleGravity();
            }
            else if (StateManager.CurrentStateFrame > es.CurrentStats.jumpVeloMinHoldFrames)
            {
                if (InputManager.GetButton((int)PlayerInputType.JUMP).isDown == false)
                {
                    (Manager as FighterManager).jumpHold = false;
                }
            }

            Vector3 movement = FighterManager.GetMovementVector();
            movement.y = 0;
            if (FighterManager.LockedOn)
            {
                FighterManager.RotateVisual(FighterManager.LockonForward, 10);
            }
            else
            {
                FighterManager.RotateVisual(movement.normalized, FighterManager.StatsManager.CurrentStats.walkRotationSpeed);
            }

            //(Manager as FighterManager).jumpTotalLength++;
            if (CheckInterrupt() == false)
            {
                StateManager.IncrementFrame();
            }
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