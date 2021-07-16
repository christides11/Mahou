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
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentForceAir"), GUIContent.none);
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentFriction"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("opponentGravity"));
            yPos += lineSpacing;
            EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, lineHeight), property.FindPropertyRelative("holdVelocityTime"));
            yPos += lineSpacing;
            return yPos;
        }
    }
}