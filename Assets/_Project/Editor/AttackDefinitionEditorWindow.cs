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
                /*
                visualFighterSceneReference.GetComponent<Fighter.FighterManager>().entityDefinition.sharedAnimations.OnEnable();
                foreach (var moveset in visualFighterSceneReference.GetComponent<Fighter.FighterManager>().entityDefinition.movesets)
                {
                    moveset.animations.OnEnable();
                }
                //visualFighterSceneReference.GetComponent<Fighter.FighterAnimator>().PlayAnimation((attack as AttackDefinition).animationName);
                visualFighterSceneReference.GetComponent<Prime31.CharacterController2D>().Awake();*/
            }
        }

        protected override void MoveEntity()
        {
            //base.MoveEntity();
            /*Vector3 mov = visualFighterSceneReference.GetComponent<FighterPhysicsManager>().GetOverallForce();
            mov *= Time.fixedDeltaTime;

            visualFighterSceneReference.transform.position += mov;*/

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

        protected override void HandleHurtboxGroup(int index, HurtboxGroup hurtboxGroup)
        {
            if (visualFighterSceneReference == null)
            {
                return;
            }

            if (timelineFrame == hurtboxGroup.activeFramesStart)
            {
                for (int i = 0; i < hurtboxGroup.boxes.Count; i++)
                {
                    Vector3 position = hurtboxGroup.attachToEntity ? ((HnSF.Combat.BoxDefinition)hurtboxGroup.boxes[i]).offset
                        : visualFighterSceneReference.transform.position + ((HnSF.Combat.BoxDefinition)hurtboxGroup.boxes[i]).offset;

                    hurtboxes.Add(new DrawnBoxDefinition()
                    {
                        attached = hurtboxGroup.attachToEntity,
                        group = index,
                        position = position,
                        size = ((HnSF.Combat.BoxDefinition)hurtboxGroup.boxes[i]).size
                    });
                }
            }

            // Remove stray hitboxes
            if (timelineFrame == hurtboxGroup.activeFramesEnd + 1)
            {
                for (int i = 0; i < hurtboxes.Count; i++)
                {
                    if (hurtboxes[i].group == index)
                    {
                        hurtboxes.RemoveAt(i);
                    }
                }
            }
        }

        protected override void HandleHitboxGroup(int index, HitboxGroup hitboxGroup)
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