using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mahou.Combat.Events
{
    [CustomPropertyDrawer(typeof(SpawnProjectile), true)]
    public class SpawnProjectileEditor : PropertyDrawer
    {
        float height = 0;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float yPosition = position.y;

            var projectilePrefabRect = new Rect(position.x, yPosition, position.width, 15);
            EditorGUI.PropertyField(projectilePrefabRect, property.FindPropertyRelative("projectilePrefab"), new GUIContent("Projectile"));
            yPosition += 15;

            height = yPosition - position.y;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }
    }
}