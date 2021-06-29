using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class HitInfo : HnSF.Combat.HitInfo
    {
        public AnimationCurve xCurvePosition;
        public AnimationCurve yCurvePosition;
        public AnimationCurve zCurvePosition;
        public AnimationCurve gravityCurve;

        public HitInfo() : base()
        {

        }

        public HitInfo(HnSF.Combat.HitInfoBase other) : base(other)
        {

        }
    }
}