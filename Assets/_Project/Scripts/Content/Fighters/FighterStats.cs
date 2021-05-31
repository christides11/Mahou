using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    [System.Serializable]
    public class FighterStats
    {
        [Header("Ground Movement")]
        public FighterStatFloat groundFriction = new FighterStatFloat(0);
        public FighterStatFloat jumpSquatFriction = new FighterStatFloat(0);

        public FighterStatFloat maxWalkSpeed = new FighterStatFloat(0);
        public FighterStatFloat walkBaseAccel = new FighterStatFloat(0);
        public FighterStatFloat walkAcceleration = new FighterStatFloat(0);
        public FighterStatFloat walkRotationSpeed = new FighterStatFloat(0);
        public AnimationCurve walkAccelFromDot = new AnimationCurve();

        public FighterStatFloat maxRunSpeed = new FighterStatFloat(0);
        public FighterStatFloat runBaseAccel = new FighterStatFloat(0);
        public FighterStatFloat runAcceleration = new FighterStatFloat(0);
        public AnimationCurve runAccelFromDot = new AnimationCurve();
        public FighterStatFloat runRotationSpeed = new FighterStatFloat(0);

        public FighterStatFloat dashInitSpeed = new FighterStatFloat(0);
        public FighterStatFloat maxDashSpeed = new FighterStatFloat(0);
        public FighterStatFloat dashAcceleration = new FighterStatFloat(0);
        public FighterStatFloat dashRotationSpeed = new FighterStatFloat(0);

        public int dashTime;

        [Header("Jump")]
        public int jumpsAvailable;
        public int jumpSquat;
        public int jumpVeloMinHoldFrames = 5;
        public int jumpVeloMaxHoldFrames = 10;
        public FighterStatFloat jumpVelocity = new FighterStatFloat(0);
        public FighterStatFloat jumpConversedMomentum = new FighterStatFloat(0);
        public FighterStatFloat jumpInitHozVelo = new FighterStatFloat(0);

        [Header("Air Jump")]
        public FighterStatFloat airJumpConversedMomentum = new FighterStatFloat(0);
        public FighterStatFloat airJumpHozVelo = new FighterStatFloat(0);
        public FighterStatFloat[] airJumpVelocity = new FighterStatFloat[0];

        [Header("Gravity")]
        public FighterStatFloat gravity = new FighterStatFloat(0);
        public FighterStatFloat maxFallSpeed = new FighterStatFloat(0);

        [Header("Air")]
        public AnimationCurve accelFromDotProduct = new AnimationCurve();
        public FighterStatFloat airBaseAccel = new FighterStatFloat(0);
        public FighterStatFloat airAccel = new FighterStatFloat(0);
        public FighterStatFloat airDeceleration = new FighterStatFloat(0);
        public FighterStatFloat maxAirSpeed = new FighterStatFloat(0);

        [Header("Air Dash")]
        public AnimationCurve airDashVelocityCurve = new AnimationCurve();
        public int airDashGravityAfter;
        public int airDashLength;

        [Header("Hitstun")]
        public FighterStatFloat hitstunGravity = new FighterStatFloat(0); //Gravity while in hitstun.
        public FighterStatFloat hitstunMaxFallSpeed = new FighterStatFloat(0);
        public FighterStatFloat hitstunFrictionXZ = new FighterStatFloat(0);
        public FighterStatFloat inertiaFriction = new FighterStatFloat(0);
        public FighterStatFloat weight = new FighterStatFloat(0);

        public FighterStats()
        {

        }

        public FighterStats(FighterStats other)
        {
            DeepCopy(other);
        }

        public void DeepCopy(FighterStats source)
        {
            // Ground Movement
            groundFriction.UpdateBaseValue(source.groundFriction);
            jumpSquatFriction.UpdateBaseValue(source.jumpSquatFriction);

            maxWalkSpeed.UpdateBaseValue(source.maxWalkSpeed);
            walkBaseAccel.UpdateBaseValue(source.walkBaseAccel);
            walkAcceleration.UpdateBaseValue(source.walkAcceleration);
            walkRotationSpeed.UpdateBaseValue(source.walkRotationSpeed);
            walkAccelFromDot = new AnimationCurve(source.walkAccelFromDot.keys);

            maxRunSpeed.UpdateBaseValue(source.maxRunSpeed);
            runBaseAccel.UpdateBaseValue(source.runBaseAccel);
            runAcceleration.UpdateBaseValue(source.runAcceleration);
            runRotationSpeed.UpdateBaseValue(source.runRotationSpeed);
            runAccelFromDot = new AnimationCurve(source.runAccelFromDot.keys);

            dashInitSpeed.UpdateBaseValue(source.dashInitSpeed);
            maxDashSpeed.UpdateBaseValue(source.maxDashSpeed);
            dashAcceleration.UpdateBaseValue(source.dashAcceleration);
            dashRotationSpeed.UpdateBaseValue(source.dashRotationSpeed);
            dashTime = source.dashTime;

            // Jump
            jumpsAvailable = source.jumpsAvailable;
            jumpSquat = source.jumpSquat;
            jumpVeloMinHoldFrames = source.jumpVeloMinHoldFrames;
            jumpVeloMaxHoldFrames = source.jumpVeloMaxHoldFrames;
            jumpVelocity.UpdateBaseValue(source.jumpVelocity);
            jumpConversedMomentum.UpdateBaseValue(source.jumpConversedMomentum);
            jumpInitHozVelo.UpdateBaseValue(source.jumpInitHozVelo);

            // Air Jump
            airJumpConversedMomentum.UpdateBaseValue(source.airJumpConversedMomentum);
            airJumpHozVelo.UpdateBaseValue(source.airJumpHozVelo);
            airJumpVelocity = new FighterStatFloat[source.airJumpVelocity.Length];
            for(int i = 0; i < airJumpVelocity.Length; i++)
            {
                airJumpVelocity[i] = source.airJumpVelocity[i];
            }

            // Gravity
            gravity = source.gravity;
            maxFallSpeed = source.maxFallSpeed;

            // Air
            accelFromDotProduct = new AnimationCurve(source.accelFromDotProduct.keys);
            airBaseAccel.UpdateBaseValue(source.airBaseAccel);
            airAccel.UpdateBaseValue(source.airAccel);
            airDeceleration.UpdateBaseValue(source.airDeceleration);
            maxAirSpeed.UpdateBaseValue(source.maxAirSpeed);

            // Air Dash
            //airDashPreFrames = source.airDashPreFrames;
            //airDashInitVelo.UpdateBaseValue(source.airDashInitVelo);
            airDashVelocityCurve = new AnimationCurve(source.airDashVelocityCurve.keys);
            //airDashFriction.UpdateBaseValue(source.airDashFriction);
            //airDashFrictionAfter = source.airDashFrictionAfter;
            airDashGravityAfter = source.airDashGravityAfter;
            airDashLength = source.airDashLength;

            // Hitstun
            hitstunGravity.UpdateBaseValue(source.hitstunGravity);
            hitstunMaxFallSpeed.UpdateBaseValue(source.hitstunMaxFallSpeed);
            hitstunFrictionXZ.UpdateBaseValue(source.hitstunFrictionXZ);
            inertiaFriction.UpdateBaseValue(source.inertiaFriction);
            weight.UpdateBaseValue(source.weight);
        }
    }
}