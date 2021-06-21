using UnityEditor;
using UnityEngine;
namespace Mahou.Combat
{
    [CustomEditor(typeof(ProjectileDefinition), true)]
    public class ProjectileDefinitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor", GUILayout.Width(Screen.width), GUILayout.Height(45)))
            {
                ProjectileDefinitionEditorWindow.Init(target as ProjectileDefinition);
            }

            DrawDefaultInspector();
        }
    }
}