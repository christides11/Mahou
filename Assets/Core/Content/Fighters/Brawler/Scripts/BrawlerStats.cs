using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BrawlerStats : MonoBehaviour
    {
        [Header("Ground Movement")]
        public float groundFriction = 1.2f;
        public float maxWalkSpeed = 8.0f;
        public float walkBaseAccel = 1.0f;
        public float walkAcceleration = 1.0f;
        public float walkRotationSpeed = 0.35f;
    }
}