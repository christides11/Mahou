using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterPhysicsManager : CAF.Fighters.FighterPhysicsManager3D
    {
        protected FighterManager Manager { get { return (FighterManager)manager; } }

        public override void Tick()
        {
            Manager.cc.SetMovement(forceMovement + forcePushbox, forceDamage, forceGravity);
            forcePushbox = Vector3.zero;
        }

        public override void Freeze()
        {
            Manager.cc.SetMovement(Vector3.zero, Vector3.zero);
        }

        public override Vector3 GetOverallForce()
        {
            return forceMovement + forceGravity + forceDamage;
        }

        public virtual void HandleGravity()
        {
            HandleGravity(Manager.StatsManager.baseStats.maxFallSpeed,
                Manager.StatsManager.baseStats.gravity, GravityScale);
        }

        public virtual void HandleGravity(float gravity)
        {
            HandleGravity(Manager.StatsManager.baseStats.maxFallSpeed, gravity, GravityScale);
        }

        public virtual void HandleGravity(float gravity, float gravityScale)
        {
            HandleGravity(Manager.StatsManager.baseStats.maxFallSpeed, gravity, gravityScale);
        }

        public override void ApplyMovementFriction(float friction = -1)
        {
            if (friction == -1)
            {
                friction = Manager.StatsManager.baseStats.groundFriction;
            }
            Vector3 realFriction = forceMovement.normalized * friction;
            forceMovement.x = ApplyFriction(forceMovement.x, Mathf.Abs(realFriction.x));
            forceMovement.z = ApplyFriction(forceMovement.z, Mathf.Abs(realFriction.z));
        }

        /// <summary>
        /// Create a force based on the parameters given and
        /// adds it to our movement force.
        /// </summary>
        /// <param name="accel">How fast the entity accelerates in the movement direction.</param>
        /// <param name="max">The max magnitude of our movement force.</param>
        /// <param name="decel">How much the entity decelerates when moving faster than the max magnitude.
        /// 1.0 = doesn't decelerate, 0.0 = force set to 0.</param>
        public virtual void ApplyMovement(float accel, float max, float decel, bool decelAtNeutral = true)
        {
            Vector2 movement = Manager.InputManager.GetAxis2D(Input.Action.Movement_X);
            // Player moving in a direction.
            if (movement.magnitude >= InputConstants.movementThreshold)
            {
                //Translate movment based on "camera."
                Vector3 translatedMovement = Manager.GetMovementVector();
                translatedMovement.y = 0;
                translatedMovement *= accel;

                forceMovement += translatedMovement;
                //Limit movement velocity.
                if (forceMovement.magnitude > max)
                {
                    forceMovement = forceMovement.normalized * max;
                }
            }
            else
            {
                if (!decelAtNeutral)
                {
                    return;
                }
                // Stick at neutral, decelerate.
                forceMovement *= decel;
                if (forceMovement.magnitude <= InputConstants.movementSigma)
                {
                    forceMovement = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Check if we are on the ground.
        /// </summary>
        public override void CheckIfGrounded()
        {
            Manager.IsGrounded = Manager.cc.Motor.GroundingStatus.IsStableOnGround;
        }
    }
}