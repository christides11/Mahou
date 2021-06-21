using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Mahou.Combat
{
    public class ProjectileEventDefinitionEditorWindow : EditorWindow
    {
        protected Dictionary<string, Type> attackEventTypes = new Dictionary<string, Type>();

        public ProjectileDefinition projectileDefinition;
        public int eventIndex;

        public static void Init(ProjectileDefinition projectileDefinition, int eventIndex)
        {
            ProjectileEventDefinitionEditorWindow window =
                (ProjectileEventDefinitionEditorWindow)
                EditorWindow.GetWindow(typeof(ProjectileEventDefinitionEditorWindow),
                true,
                ""
                );
            window.projectileDefinition = projectileDefinition;
            window.eventIndex = eventIndex;
            window.Show();
        }

        protected virtual void OnFocus()
        {
            attackEventTypes.Clear();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var givenType in a.GetTypes())
                {
                    if (givenType.IsSubclassOf(typeof(ProjectileEvent)))
                    {
                        attackEventTypes.Add(givenType.FullName, givenType);
                    }
                }
            }
        }

        protected Vector2 scrollPos;
        protected virtual void OnGUI()
        {
            if (projectileDefinition == null || projectileDefinition.events.Count <= eventIndex)
            {
                Close();
                return;
            }
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            DrawEventInfo();
            EditorGUILayout.EndScrollView();
        }

        protected virtual void DrawEventInfo()
        {
            SerializedObject projectileObj = new SerializedObject(projectileDefinition);
            projectileObj.Update();

            SerializedProperty eventProperty = projectileObj.FindProperty("events").GetArrayElementAtIndex(eventIndex);

            EditorGUILayout.PropertyField(eventProperty.FindPropertyRelative("nickname"), new GUIContent("Nickname"));
            EditorGUILayout.PropertyField(eventProperty.FindPropertyRelative("active"), new GUIContent("Active"));

            var attackEventProperty = eventProperty.FindPropertyRelative("projectileEvent");
            if (GUILayout.Button("Set Event"))
            {
                GenericMenu menu = new GenericMenu();

                foreach (string t in attackEventTypes.Keys)
                {
                    string destination = t.Replace('.', '/');
                    menu.AddItem(new GUIContent(destination), true, OnProjectileEventSelected, t);
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.LabelField(projectileDefinition.events[eventIndex].projectileEvent == null ? 
                "..." 
                : projectileDefinition.events[eventIndex].projectileEvent.GetName());
            GUILayout.Space(5);

            if (projectileDefinition.events[eventIndex].projectileEvent != null)
            {
                EditorGUILayout.PropertyField(attackEventProperty, true);
            }

            projectileObj.ApplyModifiedProperties();
        }

        protected virtual void OnProjectileEventSelected(object t)
        {
            SerializedObject attackObject = new SerializedObject(projectileDefinition);
            attackObject.Update();
            SerializedProperty eventProperty = attackObject.FindProperty("events").GetArrayElementAtIndex(eventIndex);
            var attackEventProperty = eventProperty.FindPropertyRelative("projectileEvent");
            attackEventProperty.managedReferenceValue = Activator.CreateInstance(attackEventTypes[(string)t]);
            attackObject.ApplyModifiedProperties();
        }
    }
}