using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterPhysicsManager : HnSF.Fighters.FighterPhysicsManager3D
    {
        public delegate void GroundedStateAction(bool state);
        public event GroundedStateAction OnGroundedChanged;

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
            forceGravity = Vector3.MoveTowards(forceGravity, 
                new Vector3(0, -Manager.StatsManager.CurrentStats.maxFallSpeed, 0), 
                Manager.StatsManager.CurrentStats.gravity * Time.fixedDeltaTime);
            //HandleGravity(Manager.StatsManager.CurrentStats.maxFallSpeed,
            //    Manager.StatsManager.CurrentStats.gravity, 1.0f);
        }

        public virtual void HandleGravity(float gravity)
        {
            HandleGravity(Manager.StatsManager.CurrentStats.maxFallSpeed, gravity, GravityScale);
        }

        public virtual void HandleGravity(float gravity, float gravityScale)
        {
            HandleGravity(Manager.StatsManager.CurrentStats.maxFallSpeed, gravity, gravityScale);
        }

        public override void ApplyMovementFriction(float friction = -1)
        {
            if (friction == -1)
            {
                friction = Manager.StatsManager.CurrentStats.groundFriction;
            }
            Vector3 realFriction = forceMovement.normalized * friction;
            forceMovement.x = ApplyFriction(forceMovement.x, Mathf.Abs(realFriction.x) * Time.fixedDeltaTime);
            forceMovement.z = ApplyFriction(forceMovement.z, Mathf.Abs(realFriction.z) * Time.fixedDeltaTime);
        }

        public virtual void HandleMovement(float baseAccel, float movementAccel, float deceleration, float maxSpeed, AnimationCurve accelFromDot)
        {
            // Get wanted movement vector.
            Vector3 movement = Manager.GetMovementVector();
            if(movement.magnitude < InputConstants.movementThreshold)
            {
                movement = Vector3.zero;
            }
            
            // Real Accel
            float realAcceleration = baseAccel + (movement.magnitude * movementAccel);

            if (movement.magnitude > 1.0f)
            {
                movement.Normalize();
            }

            // Calculated our wanted movement force.
            float accel = movement == Vector3.zero ? deceleration : realAcceleration * accelFromDot.Evaluate(Vector3.Dot(movement, forceMovement.normalized));
            Vector3 goalVelocity = movement * maxSpeed;

            // Move towards that goal based on our acceleration.
            forceMovement = Vector3.MoveTowards(forceMovement, goalVelocity, accel * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Check if we are on the ground.
        /// </summary>
        public override void CheckIfGrounded()
        {
            bool currentGroundState = IsGrounded;
            IsGrounded = Manager.cc.Motor.GroundingStatus.IsStableOnGround;
            if(IsGrounded != currentGroundState)
            {
                OnGroundedChanged?.Invoke(IsGrounded);
            }
        }
    }
}