using HnSF.Combat;
using KinematicCharacterController;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mahou.Combat
{
    public class AttackDefinitionEditorWindow : HnSF.Combat.AttackDefinitionEditorWindow
    {
        public struct DrawnBoxDefinition
        {
            public int group;
            public bool attached;
            public Vector3 position;
            public Vector3 size;
        }

        public List<DrawnBoxDefinition> hitboxes = new List<DrawnBoxDefinition>();
        public List<DrawnBoxDefinition> hurtboxes = new List<DrawnBoxDefinition>();

        public KinematicCharacterSystem localSystem;

        public static void Init(AttackDefinition attack)
        {
            AttackDefinitionEditorWindow window = ScriptableObject.CreateInstance<AttackDefinitionEditorWindow>();
            window.attack = attack;
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            hitboxes.Clear();
            hurtboxes.Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (visualFighterSceneReference)
            {
                KinematicCharacterSystem.UnregisterCharacterMotor(visualFighterSceneReference.GetComponent<KinematicCharacterMotor>());
            }
        }

        protected override void ResetFighterVariables()
        {
            hitboxes.Clear();
            hurtboxes.Clear();
            visualFighterSceneReference.transform.position = new Vector3(0, 0, 0);
            visualFighterSceneReference.GetComponent<KinematicCharacterMotor>().Awake();
            visualFighterSceneReference.GetComponent<KinematicCharacterMotor>().SetPosition(new Vector3(0, 0, 0), true);
        }

        protected override void CreateFighter()
        {
            base.CreateFighter();
            if(localSystem != null)
            {
                DestroyImmediate(localSystem.gameObject);
            }
            GameObject kcs = new GameObject();
            kcs.AddComponent<KinematicCharacterSystem>();
            renderUtils.AddSingleGO(kcs);
            localSystem = kcs.GetComponent<KinematicCharacterSystem>();
            KinematicCharacterSystem.ForceInstance(localSystem);
            KinematicCharacterSystem.CharacterMotors.Clear();

            if (visualFighterSceneReference)
            {
                KinematicCharacterSystem.CharacterMotors.Add(visualFighterSceneReference.GetComponent<KinematicCharacterMotor>());
                visualFighterSceneReference.GetComponent<KinematicCharacterMotor>().CharacterController =
                    visualFighterSceneReference.GetComponent<FighterCharacterController>();
                visualFighterSceneReference.GetComponent<KinematicCharacterMotor>().Awake();
            }
        }

        protected override void MoveEntity()
        {
            FighterPhysicsManager pm = visualFighterSceneReference.GetComponent<FighterPhysicsManager>();
            pm.Tick();

            KinematicCharacterSystem.PreSimulationInterpolationUpdate(Time.fixedDeltaTime);
            KinematicCharacterSystem.Simulate(Time.fixedDeltaTime, KinematicCharacterSystem.CharacterMotors, KinematicCharacterSystem.PhysicsMovers);
            KinematicCharacterSystem.PostSimulationInterpolationUpdate(Time.fixedDeltaTime);
        }

        protected override void DrawHitboxes()
        {
            if (visualFighterSceneReference == null)
            {
                return;
            }
            Handles.color = Color.green;
            foreach (var hitbox in hitboxes)
            {
                if (hitbox.attached)
                {
                    Handles.DrawWireCube(visualFighterSceneReference.transform.position + (Vector3)hitbox.position, hitbox.size);
                }
                else
                {
                    Handles.DrawWireCube(hitbox.position, hitbox.size);
                }
            }
        }

        protected override void DrawHurtboxes()
        {
            if (visualFighterSceneReference == null)
            {
                return;
            }
            Handles.color = Color.blue;
            foreach (var hurtbox in hurtboxes)
            {
                if (hurtbox.attached)
                {
                    Handles.DrawWireCube(visualFighterSceneReference.transform.position + (Vector3)hurtbox.position, hurtbox.size);
                }
                else
                {
                    Handles.DrawWireCube(hurtbox.position, hurtbox.size);
                }
            }
        }

        protected override void DrawHurtboxDefinition(HnSF.Combat.StateHurtboxDefinition hurtboxDefinition)
        {
            Debug.Log("2.");
            if (visualFighterSceneReference == null)
            {
                return;
            }

            if (hurtboxDefinition == null)
            {
                return;
            }

            for (int i = 0; i < hurtboxDefinition.hurtboxGroups.Count; i++)
            {
                if (timelineFrame == hurtboxDefinition.hurtboxGroups[i].activeFramesStart
                    || hurtboxDefinition.hurtboxGroups[i].activeFramesEnd == -1)
                {
                    for (int w = 0; w < hurtboxDefinition.hurtboxGroups[i].boxes.Count; w++)
                    {

                        Vector3 position = hurtboxDefinition.hurtboxGroups[i].attachToEntity
                            ? ((HnSF.Combat.BoxDefinition)hurtboxDefinition.hurtboxGroups[i].boxes[w]).offset
                            : visualFighterSceneReference.transform.position + ((HnSF.Combat.BoxDefinition)hurtboxDefinition.hurtboxGroups[i].boxes[w]).offset;

                        hurtboxes.Add(new DrawnBoxDefinition()
                        {
                            attached = hurtboxDefinition.hurtboxGroups[i].attachToEntity,
                            group = i,
                            position = position,
                            size = ((HnSF.Combat.BoxDefinition)hurtboxDefinition.hurtboxGroups[i].boxes[w]).size
                        });
                    }
                }

                // Remove stray hitboxes
                if (hurtboxDefinition.hurtboxGroups[i].activeFramesEnd != -1 && timelineFrame == hurtboxDefinition.hurtboxGroups[i].activeFramesEnd + 1)
                {
                    for (int w = 0; w < hurtboxes.Count; w++)
                    {
                        if (hurtboxes[w].group == i)
                        {
                            hurtboxes.RemoveAt(i);
                        }
                    }
                }
            }
        }

        protected override void DrawHitboxGroup(int index, HitboxGroup hitboxGroup)
        {
            if (visualFighterSceneReference == null)
            {
                return;
            }

            // Create the hitboxes
            if (timelineFrame == hitboxGroup.activeFramesStart)
            {
                for (int i = 0; i < hitboxGroup.boxes.Count; i++)
                {
                    Vector3 position = hitboxGroup.attachToEntity ? ((HnSF.Combat.BoxDefinition)hitboxGroup.boxes[i]).offset
                        : visualFighterSceneReference.transform.position + ((HnSF.Combat.BoxDefinition)hitboxGroup.boxes[i]).offset;

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

    }
}