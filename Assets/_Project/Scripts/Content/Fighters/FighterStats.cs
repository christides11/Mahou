using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    [System.Serializable]
    public struct FighterStats
    {

        [Header("Ground Movement")]
        public float groundFriction;
        public float maxWalkSpeed;
        public float walkBaseAccel;
        public float walkAcceleration;
        public float walkRotationSpeed;
        public float maxRunSpeed;
        public float runBaseAccel;
        public float runAcceleration;
        public float runRotationSpeed;
        public float dashInitSpeed;
        public float maxDashSpeed;
        public float dashAcceleration;
        public int dashTime;

        [Header("Jump")]
        public int jumpsAvailable;
        public int jumpSquat;
        public float shortHopVelocity;
        public float fullHopVelocity;
        public float jumpConversedMomentum;
        public float jumpInitHozVelo;

        [Header("Air Jump")]
        public float airJumpConversedMomentum;
        public float airJumpHozVelo;
        public float[] airJumpVelocity;

        [Header("Gravity")]
        public float gravity;
        public float maxFallSpeed;

        [Header("Air")]
        public float airAcceleration;
        public float airDeceleration;
        public float maxAirSpeed;

        [Header("Air Dash")]
        public int airDashPreFrames;
        public float airDashInitVelo;
        public float airDashFriction;
        public int airDashFrictionAfter;
        public int airDashGravityAfter;
        public int airDashLength;

        [Header("Hitstun")]
        public float hitstunGravity; //Gravity while in hitstun.
        public float hitstunMaxFallSpeed;
        public float hitstunFrictionXZ;
        public float inertiaFriction;
        public float weight;
    }
}