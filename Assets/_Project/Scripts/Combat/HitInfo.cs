using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    [System.Serializable]
    public class HitInfo : HnSF.Combat.HitInfo
    {
        public Vector3 opponentForceAir;
        public float opponentFriction;
        public float opponentGravity;


        public HitInfo() : base()
        {

        }

        public HitInfo(HnSF.Combat.HitInfoBase other) : base(other)
        {

        }
    }
}