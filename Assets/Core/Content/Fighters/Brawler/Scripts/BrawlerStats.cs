using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BrawlerStats : FighterStats
    {
        [Header("Ground Movement")]
        public float maxWalkSpeed = 8.0f;
        public float walkBaseAccel = 1.0f;
        public float walkAcceleration = 1.0f;
        public float walkRotationSpeed = 0.35f;
    }
}