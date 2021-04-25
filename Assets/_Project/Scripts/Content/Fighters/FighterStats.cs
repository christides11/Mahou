using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStats : MonoBehaviour
    {
        public int jumpSquat = 5;

        [Header("Ground Movement")]
        public float groundFriction = 0.1f;
        public float maxWalkSpeed = 8.0f;
        public float walkBaseAccel = 1.0f;
        public float walkAcceleration = 1.0f;
        public float walkRotationSpeed = 0.35f;
        public float maxRunSpeed = 20;
        public float runBaseAccel = 2.0f;
        public float runAcceleration = 2.5f;
        public float runRotationSpeed = 0.97f;
        public float dashInitSpeed = 5;
        public float maxDashSpeed = 20;
        public float dashAcceleration = 1;
        public int dashTime = 20;

        [Header("Jump")]
        public int jumpsquat = 5;
        public float shortHopVelocity = 13;
        public float fullHopVelocity = 16;
        public float jumpConversedMomentum = 1;
        public float jumpInitHozVelo = 8;

        [Header("Gravity")]
        public float gravity = 0;
        public float maxFallSpeed = 0;

        [Header("Air")]
        public float airAcceleration = 0;
        public float airDeceleration = 0;
        public float maxAirSpeed = 0;

        [Header("Air Dash")]
        public int airDashPreFrames = 3;
        public float airDashInitVelo = 20;
        public float airDashFriction = 0.25f;
        public int airDashFrictionAfter = 10;
        public int airDashGravityAfter = 10;
        public int airDashLength = 25;

        [Header("Hitstun")]
        public float hitstunGravity; //Gravity while in hitstun.
        public float hitstunMaxFallSpeed;
        public float hitstunFrictionXZ;
        public float inertiaFriction;
        public float weight = 1;
    }
}