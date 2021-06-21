using HnSF.Combat;
using KinematicCharacterController;
using Mahou.Content.Fighters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mahou.Combat
{
    public class ProjectileDefinitionEditorWindow : EditorWindow
    {
        public struct DrawnBoxDefinition
        {
            public int group;
            public bool attached;
            public Vector3 position;
            public Vector3 size;
        }

        protected PreviewRenderUtility renderUtils;
        public ProjectileDefinition projectileDefinition;

        public Projectile projectileScenePrefab;
        public Projectile projectileSceneObject;

        public int timelineFrame;

        protected bool autoplay;
        protected double playInterval;
        protected double nextPlayTime = 0;

        public bool showHitboxGroups = true;
        public bool showEvents = true;

        public static void Init(ProjectileDefinition projectile)
        {
            ProjectileDefinitionEditorWindow window = ScriptableObject.CreateInstance<ProjectileDefinitionEditorWindow>();
            window.projectileDefinition = projectile;
            window.Show();
        }

        protected Dictionary<string, Type> attackEventDefinitionTypes = new Dictionary<string, Type>();
        protected Dictionary<string, Type> hitboxGroupTypes = new Dictionary<string, Type>();
        protected virtual void OnFocus()
        {
            attackEventDefinitionTypes.Clear();
            hitboxGroupTypes.Clear();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var givenType in a.GetTypes())
                {
                    if (givenType == typeof(ProjectileEventDefinition) || givenType.IsSubclassOf(typeof(ProjectileEventDefinition)))
                    {
                        attackEventDefinitionTypes.Add(givenType.FullName, givenType);
                    }
                    if (givenType == typeof(HitboxGroup) || givenType.IsSubclassOf(typeof(HitboxGroup)))
                    {
                        hitboxGroupTypes.Add(givenType.FullName, givenType);
                    }
                }
            }
        }

        protected virtual void OnEnable()
        {
            if (renderUtils == null)
            {
                renderUtils = new PreviewRenderUtility(true);
            }
            renderUtils.camera.cameraType = CameraType.SceneView;
            renderUtils.camera.transform.position = new Vector3(0, 1, -10);
            renderUtils.camera.transform.LookAt(new Vector3(0, 1, 0));
            renderUtils.camera.farClipPlane = 100;
            hitboxes.Clear();
        }

        protected virtual void OnDisable()
        {
            if (renderUtils != null)
            {
                renderUtils.Cleanup();
            }
        }

        public virtual void Update()
        {
            Repaint();
        }

        protected Vector2 scroll;
        protected bool rotationMode = false;
        protected bool moveMode = false;
        protected Vector2 mousePos = new Vector2(0, 0);
        protected Vector2 diff = Vector2.zero;
        protected float rotSpeed = 1;
        protected float scrollWheel = 0;
        protected float scrollSpeed = 0.5f;
        protected float moveSpeed = 0.1f;
        public virtual void OnGUI()
        {
            if (projectileDefinition == null)
            {
                Close();
                return;
            }
            var pos = position;
            pos.x = 0;
            pos.y = 0;
            pos.height /= 2.75f;
            pos.height = Mathf.Min(250, pos.height);

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (pos.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.button == 0)
                        {
                            mousePos = Event.current.mousePosition;
                            moveMode = true;
                        }
                        else if (Event.current.button == 1)
                        {
                            mousePos = Event.current.mousePosition;
                            rotationMode = true;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (Event.current.button == 0)
                    {
                        moveMode = false;
                    }
                    if (Event.current.button == 1)
                    {
                        rotationMode = false;
                    }
                    break;
                case EventType.MouseDrag:
                    if (rotationMode || moveMode)
                    {
                        diff = Event.current.mousePosition - mousePos;
                        mousePos = Event.current.mousePosition;
                    }
                    break;
                case EventType.ScrollWheel:
                    if (rotationMode)
                    {
                        scrollWheel = Event.current.delta.y;
                    }
                    break;
            }

            renderUtils.BeginPreview(pos, EditorStyles.helpBox);
            renderUtils.Render(true, false);
            DrawGround();
            //DrawHurtboxes();
            DrawHitboxes();
            renderUtils.EndAndDrawPreview(pos);

            if (scrollWheel != 0)
            {
                renderUtils.camera.transform.position += renderUtils.camera.transform.forward * scrollWheel * scrollSpeed;
                scrollWheel = 0;
            }

            if (diff.magnitude > 0)
            {
                if (moveMode)
                {
                    renderUtils.camera.transform.position += new Vector3(0, diff.y * moveSpeed * Time.deltaTime, 0);
                }
                if (rotationMode)
                {
                    renderUtils.camera.transform.RotateAround(new Vector3(0, renderUtils.camera.transform.position.y, 0), Vector3.up, diff.x * rotSpeed);
                }
                diff = Vector2.zero;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("fr:", GUILayout.Width(15));
            GUILayout.Label(timelineFrame.ToString(), GUILayout.Width(20));
            GUILayout.Label("/", GUILayout.Width(10));
            GUILayout.Label(projectileDefinition.length.ToString(), GUILayout.Width(55));
            EditorGUILayout.EndHorizontal();

            SerializedObject serializedObject = new SerializedObject(projectileDefinition);
            serializedObject.Update();
            GUILayout.BeginArea(new Rect(pos.x, pos.y + pos.height, pos.width, position.height - pos.height));

            DrawGeneralOptions(serializedObject);
            if (projectileDefinition.scriptOverride)
            {
                GUILayout.EndArea();
                serializedObject.ApplyModifiedProperties();
                return;
            }
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Profiler.PrevFrame"), GUILayout.Width(30)))
            {
                timelineFrame = Mathf.Max(0, timelineFrame - 1);
                FastForward();
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Profiler.NextFrame"), GUILayout.Width(30)))
            {
                IncrementForward();
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_preAudioAutoPlayOff"), GUILayout.Width(30), GUILayout.Height(18)))
            {
                FastForward();
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Profiler.LastFrame"), GUILayout.Width(30)))
            {
                autoplay = !autoplay;
                if (autoplay)
                {
                    nextPlayTime = EditorApplication.timeSinceStartup + playInterval;
                }
            }
            if (autoplay && projectileSceneObject != null && EditorApplication.timeSinceStartup >= nextPlayTime)
            {
                if (timelineFrame + 1 > projectileDefinition.length)
                {
                    timelineFrame = 0;
                    FastForward();
                }
                else
                {
                    IncrementForward();
                }
                nextPlayTime = EditorApplication.timeSinceStartup + playInterval;
            }
            timelineFrame = (int)EditorGUILayout.Slider(timelineFrame, 0, projectileDefinition.length);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (showHitboxGroups)
            {
                DrawHitboxGroupBars(serializedObject);
            }
            GUILayout.Space(10);
            if (showEvents)
            {
                DrawEventBars(serializedObject);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHitboxes()
        {
            if (projectileSceneObject == null)
            {
                return;
            }
            Handles.color = Color.green;
            foreach (var hitbox in hitboxes)
            {
                if (hitbox.attached)
                {
                    Handles.DrawWireCube(projectileSceneObject.transform.position + (Vector3)hitbox.position, hitbox.size);
                }
                else
                {
                    Handles.DrawWireCube(hitbox.position, hitbox.size);
                }
            }
        }

        #region Play
        private void IncrementForward()
        {
            timelineFrame = Mathf.Min(timelineFrame + 1, projectileDefinition.length);

            if (projectileSceneObject == null)
            {
                return;
            }
            
            for (int i = 0; i < projectileDefinition.hitboxGroups.Count; i++)
            {
                DrawHitboxGroup(i, projectileDefinition.hitboxGroups[i]);
            }
            //DrawHurtboxDefinition(attack.hurtboxDefinition);
            for (int i = 0; i < projectileDefinition.events.Count; i++)
            {
                HandleEvent(projectileDefinition.events[i]);
            }
            for (int i = 0; i < projectileSceneObject.particleSystems.Length; i++)
            {
                projectileSceneObject.particleSystems[i].Simulate((float)timelineFrame * Time.fixedDeltaTime, true, true);
                projectileSceneObject.particleSystems[i].Pause();
            }
            Repaint();
            MoveEntity();
        }

        private void FastForward()
        {
            if (projectileSceneObject == null)
            {
                return;
            }

            ResetSceneProjectile();

            if (timelineFrame == 0)
            {
                return;
            }
            int oldFrame = timelineFrame;
            timelineFrame = 0;

            for (int i = 0; i < oldFrame; i++)
            {
                IncrementForward();
            }
        }
        #endregion

        public List<DrawnBoxDefinition> hitboxes = new List<DrawnBoxDefinition>();
        protected virtual void DrawHitboxGroup(int index, HitboxGroup hitboxGroup)
        {
            if(projectileSceneObject == null)
            {
                return;
            }

            // Create the hitboxes
            if (timelineFrame == hitboxGroup.activeFramesStart)
            {
                for (int i = 0; i < hitboxGroup.boxes.Count; i++)
                {
                    Vector3 position = hitboxGroup.attachToEntity ? ((HnSF.Combat.BoxDefinition)hitboxGroup.boxes[i]).offset
                        : projectileSceneObject.transform.position + ((HnSF.Combat.BoxDefinition)hitboxGroup.boxes[i]).offset;

                    hitboxes.Add(new DrawnBoxDefinition()
                    {
                        attached = hitboxGroup.attachToEntity,
                        group = index,
                        position = position,
                        size = ((HnSF.Combat.BoxDefinition)hitboxGroup.boxes[i]).size
                    });
                }
            }

            // Remove stray hitboxes
            if (timelineFrame == hitboxGroup.activeFramesEnd + 1)
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    if (hitboxes[i].group == index)
                    {
                        hitboxes.RemoveAt(i);
                    }
                }
            }
        }

        private void HandleEvent(object p)
        {

        }

        private void MoveEntity()
        {

        }

        private void DrawHitboxGroupBars(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Hitbox Groups", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.MaxWidth(30)))
            {
                GenericMenu menu = new GenericMenu();

                foreach (string t in hitboxGroupTypes.Keys)
                {
                    string destination = t.Replace('.', '/');
                    menu.AddItem(new GUIContent(destination), true, OnHitboxGroupSelected, t);
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            var hitboxGroupProperty = serializedObject.FindProperty("hitboxGroups");
            for (int i = 0; i < hitboxGroupProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(25);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    hitboxGroupProperty.DeleteArrayElementAtIndex(i);
                    return;
                }
                if (GUILayout.Button("↑", GUILayout.Width(20)))
                {
                    if (i != 0)
                    {
                        hitboxGroupProperty.MoveArrayElement(i, i - 1);
                        return;
                    }
                }
                if (GUILayout.Button("↓", GUILayout.Width(20)))
                {
                    if (i != hitboxGroupProperty.arraySize - 1)
                    {
                        hitboxGroupProperty.MoveArrayElement(i, i + 1);
                        return;
                    }
                }
                SerializedProperty arrayElement = hitboxGroupProperty.GetArrayElementAtIndex(i);
                float activeFramesStart = arrayElement.FindPropertyRelative("activeFramesStart").intValue;
                float activeFramesEnd = arrayElement.FindPropertyRelative("activeFramesEnd").intValue;
                EditorGUILayout.LabelField($"{activeFramesStart.ToString("F0")}~{activeFramesEnd.ToString("F0")}", GUILayout.Width(55));
                if (GUILayout.Button("Info", GUILayout.Width(100)))
                {
                    OpenHitboxGroupWindow(projectileDefinition, projectileDefinition.hitboxGroups[i], "hitboxGroups", i);
                }
                EditorGUILayout.MinMaxSlider(ref activeFramesStart,
                    ref activeFramesEnd,
                    1,
                    serializedObject.FindProperty("length").intValue);
                arrayElement.FindPropertyRelative("activeFramesStart").intValue = (int)activeFramesStart;
                arrayElement.FindPropertyRelative("activeFramesEnd").intValue = (int)activeFramesEnd;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void OpenHitboxGroupWindow(ProjectileDefinition projectileDefinition, HitboxGroup hitboxGroup, string v, int i)
        {
            HitboxGroupEditorWindow.Init(projectileDefinition, hitboxGroup, v, i);
        }

        protected virtual void OnHitboxGroupSelected(object t)
        {
            SerializedObject projectileObj = new SerializedObject(projectileDefinition);
            projectileObj.Update();
            SerializedProperty hitboxGroupsProperty = projectileObj.FindProperty("hitboxGroups");
            hitboxGroupsProperty.InsertArrayElementAtIndex(hitboxGroupsProperty.arraySize);
            hitboxGroupsProperty.GetArrayElementAtIndex(hitboxGroupsProperty.arraySize - 1).managedReferenceValue = Activator.CreateInstance(hitboxGroupTypes[(string)t]);
            projectileObj.ApplyModifiedProperties();
        }

        Projectile tempProjectile;
        protected virtual void DrawGeneralOptions(SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileName"), new GUIContent("Name"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("length"));
            EditorGUILayout.BeginHorizontal();
            tempProjectile = (Projectile)EditorGUILayout.ObjectField("Projectile", tempProjectile, typeof(Projectile), false);
            if (GUILayout.Button("Apply"))
            {
                CreateProjectile();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scriptOverride"), new GUIContent("Script Override"));
            EditorGUI.BeginDisabledGroup(!serializedObject.FindProperty("scriptOverride").boolValue);
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("stateOverride"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEventBars(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Events", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.MaxWidth(30)))
            {
                GenericMenu menu = new GenericMenu();

                foreach (string t in attackEventDefinitionTypes.Keys)
                {
                    string destination = t.Replace('.', '/');
                    menu.AddItem(new GUIContent(destination), true, OnAttackEventSelected, t);
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            //DrawUILine(Color.gray);
            var eventsProperty = serializedObject.FindProperty("events");
            for (int i = 0; i < eventsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(25);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    eventsProperty.DeleteArrayElementAtIndex(i);
                    return;
                }
                if (GUILayout.Button("↑", GUILayout.Width(20)))
                {
                    if (i != 0)
                    {
                        eventsProperty.MoveArrayElement(i, i - 1);
                        return;
                    }
                }
                if (GUILayout.Button("↓", GUILayout.Width(20)))
                {
                    if (i != eventsProperty.arraySize - 1)
                    {
                        eventsProperty.MoveArrayElement(i, i + 1);
                        return;
                    }
                }
                
                SerializedProperty arrayElement = eventsProperty.GetArrayElementAtIndex(i);
                float activeFramesStart = arrayElement.FindPropertyRelative("startFrame").intValue;
                float activeFramesEnd = arrayElement.FindPropertyRelative("endFrame").intValue;
                EditorGUILayout.LabelField($"{activeFramesStart.ToString("F0")}~{activeFramesEnd.ToString("F0")}", GUILayout.Width(55));
                if (GUILayout.Button(arrayElement.FindPropertyRelative("nickname").stringValue, GUILayout.Width(100)))
                {
                    OpenEventDefinitionWindow(projectileDefinition, i);
                }
                EditorGUILayout.MinMaxSlider(ref activeFramesStart,
                    ref activeFramesEnd,
                    1,
                    serializedObject.FindProperty("length").intValue);
                arrayElement.FindPropertyRelative("startFrame").intValue = (int)activeFramesStart;
                arrayElement.FindPropertyRelative("endFrame").intValue = (int)activeFramesEnd;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void OpenEventDefinitionWindow(ProjectileDefinition projectileDefinition, int i)
        {
            ProjectileEventDefinitionEditorWindow.Init(projectileDefinition, i);
        }

        protected virtual void OnAttackEventSelected(object t)
        {
            SerializedObject projectileObj = new SerializedObject(projectileDefinition);
            projectileObj.Update();
            SerializedProperty eventProperty = projectileObj.FindProperty("events");
            eventProperty.InsertArrayElementAtIndex(eventProperty.arraySize);
            eventProperty.GetArrayElementAtIndex(eventProperty.arraySize - 1).managedReferenceValue = Activator.CreateInstance(attackEventDefinitionTypes[(string)t]);
            projectileObj.ApplyModifiedProperties();
        }

        protected virtual void CreateProjectile()
        {
            if (projectileSceneObject != null)
            {
                DestroyImmediate(projectileSceneObject);
                projectileSceneObject = null;
            }
            projectileScenePrefab = tempProjectile;
            projectileSceneObject = renderUtils.InstantiatePrefabInScene(projectileScenePrefab.gameObject).GetComponent<Projectile>();
            ResetSceneProjectile();
        }

        protected virtual void ResetSceneProjectile()
        {
            hitboxes.Clear();
            projectileSceneObject.transform.position = Vector3.zero;
            projectileSceneObject.transform.eulerAngles = Vector3.zero;
            for (int i = 0; i < projectileSceneObject.particleSystems.Length; i++)
            {
                projectileSceneObject.particleSystems[i].time = 0;
            }
        }

        protected virtual void DrawGround()
        {
            Color grey = Color.grey + new Color(0, 0, 0, -0.5f);
            Handles.SetCamera(renderUtils.camera);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.color = grey;
            for (int i = -10; i <= 10; i++)
            {
                Handles.color = i == 0 ? Color.red : grey;
                Handles.DrawLine(new Vector3(i, 0, -10), new Vector3(i, 0, 10));
                Handles.color = i == 0 ? Color.green : grey;
                Handles.DrawLine(new Vector3(-10, 0, i), new Vector3(10, 0, i));
            }
            Handles.color = Color.cyan;
            Handles.DrawLine(Vector3.zero, new Vector3(0, 10, 0));
        }
    }
}