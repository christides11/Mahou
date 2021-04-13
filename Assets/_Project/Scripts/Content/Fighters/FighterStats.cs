using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStats : MonoBehaviour
    {
        public int jumpSquat = 5;

        [Header("Gravity")]
        public float gravity = 0;
        public float maxFallSpeed = 0;

        [Header("Air")]
        public float airAcceleration = 0;
        public float airDeceleration = 0;
        public float maxAirSpeed = 0;

        [Header("Jump")]
        public float shortHopVelocity = 0;
        public float fullHopVelocity = 0;
        public float jumpConversedMomentum = 1;

        [Header("Ground")]
        public float groundFriction = 0.1f;
    }
}