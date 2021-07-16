using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    [System.Flags]
    public enum InvincibilityTypes
    {
        NONE = 0,
        STRIKE = 1,
        PROJECTILE = 2,
        THROW = 4
    }

    public enum ArmorType
    {
        NONE,
        SUPER,
        HYPER,
        GUARD_POINT,
        PARRY
    }

    public class HurtboxGroup : HnSF.Combat.HurtboxGroup
    {
        public InvincibilityTypes invincibility;
        public ArmorType armor;
    }
}