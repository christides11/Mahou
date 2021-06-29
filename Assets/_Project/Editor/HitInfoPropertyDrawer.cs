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

            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("xCurvePosition"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("yCurvePosition"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("zCurvePosition"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("gravityCurve"));
            yPos += lineSpacing;

            return yPos;
        }
    }
}