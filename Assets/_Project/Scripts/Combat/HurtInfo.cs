using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class HurtInfo : HnSF.Combat.HurtInfo3D
    {


        public HurtInfo(HitInfo hitInfo, Vector3 center, Vector3 forward, Vector3 right) : base(hitInfo, center, forward, right)
        {

        }
    }
}