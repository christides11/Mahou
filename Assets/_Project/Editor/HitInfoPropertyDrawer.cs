using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mahou.Combat
{
    [CustomPropertyDrawer(typeof(Mahou.Combat.HitInfo), true)]
    public class HitInfoPropertyDrawer : HnSF.Combat.HitInfoPropertyDrawer
    {
        protected override float DrawForcesGroup(ref Rect position, SerializedProperty property, float yPosition)
        {
            float yPos = base.DrawForcesGroup(ref position, property, yPosition);
            return yPos;
        }
    }
}