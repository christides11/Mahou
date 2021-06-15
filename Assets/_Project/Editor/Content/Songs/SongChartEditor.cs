using UnityEditor;
using UnityEngine;

namespace Mahou.Content
{
    [CustomEditor(typeof(SongChart), true)]
    public class SongChartEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor", GUILayout.Width(Screen.width), GUILayout.Height(45)))
            {
                SongChartEditorWindow.Init(target as SongChart);
            }

            DrawDefaultInspector();
        }
    }
}