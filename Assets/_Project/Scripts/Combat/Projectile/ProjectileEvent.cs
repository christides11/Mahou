using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class ProjectileEvent
    {
        public virtual string GetName()
        {
            return "Event";
        }

        public virtual AttackEventReturnType Evaluate(int frame, int length, Projectile projectile)
        {
            return AttackEventReturnType.NONE;
        }
    }
}