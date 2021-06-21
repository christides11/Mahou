using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mahou.Combat.AttackEvents
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
            var spawnAtEnemyRect = new Rect(position.x, yPosition, position.width, 15);
            EditorGUI.PropertyField(spawnAtEnemyRect, property.FindPropertyRelative("spawnAtEnemy"), new GUIContent("Spawn At Enemy"));
            yPosition += 15;
            if(property.FindPropertyRelative("spawnAtEnemy").boolValue == true)
            {
                var maxDistanceRect = new Rect(position.x, yPosition, position.width, 15);
                EditorGUI.PropertyField(maxDistanceRect, property.FindPropertyRelative("maxDistance"), new GUIContent("Max Distance"));
                yPosition += 15;
            }
            var offsetRect = new Rect(position.x, yPosition, position.width, 15);
            EditorGUI.PropertyField(offsetRect, property.FindPropertyRelative("offset"), new GUIContent("Spawn Offset"));
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