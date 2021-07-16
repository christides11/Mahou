using Mahou.Content.Fighters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class FighterPushboxManager : MonoBehaviour
    {
        [SerializeField] protected FighterManager manager;

        protected StateHurtboxDefinition hurtboxDefinition;

        protected List<Pushbox> pushboxPool = new List<Pushbox>();
        protected Dictionary<int, List<Pushbox>> pushboxGroups = new Dictionary<int, List<Pushbox>>();

        [SerializeField] protected Pushbox pushboxPrefab;

        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
        }

        public virtual void Cleanup()
        {
            foreach (int id in pushboxGroups.Keys)
            {
                for (int i = 0; i < pushboxGroups[id].Count; i++)
                {
                    DestroyHurtbox(pushboxGroups[id][i]);
                }
            }
            pushboxGroups.Clear();
            hurtboxDefinition = null;
        }

        public virtual void Tick()
        {
            CreatePushboxes(hurtboxDefinition, manager.StateManager.CurrentStateFrame);
        }

        Vector3 pushDir;
        float pushFor;
        public float pushboxForceModifier = 1.0f;
        public virtual void LateTick()
        {
            foreach (var p in pushboxGroups.Values)
            {
                for(int i = 0; i < p.Count; i++)
                {
                    if (p[i].forceFrame)
                    {
                        pushDir = p[i].dir;
                        pushFor = p[i].forc;
                    }
                    p[i].forceFrame = false;
                }
            }

            if (pushFor != 0)
            {
                manager.cc.AddVelocity(pushDir * pushFor * pushboxForceModifier);
            }
            pushFor = 0;
        }

        protected List<int> hurtboxGroupsToDelete = new List<int>();
        public virtual void CreatePushboxes(StateHurtboxDefinition currentHurtboxDefinition, uint frame)
        {
            this.hurtboxDefinition = currentHurtboxDefinition;

            for (int i = 0; i < currentHurtboxDefinition.hurtboxGroups.Count; i++)
            {
                if (!pushboxGroups.ContainsKey(i))
                {
                    pushboxGroups.Add(i, new List<Pushbox>());
                }

                // STRAY HURTBOX CLEANUP //
                for (int s = currentHurtboxDefinition.hurtboxGroups[i].boxes.Count; s < pushboxGroups[i].Count; s++)
                {
                    DestroyHurtbox(pushboxGroups[i][s]);
                    pushboxGroups[i].RemoveAt(s);
                }

                // CHECK CLEANUP WINDOW //
                if (CheckWithinHurtboxWindow(currentHurtboxDefinition, i, frame) == false)
                {
                    CleanupHurtboxGroup(i);
                    continue;
                }

                // CREATE HURTBOX GROUPS //
                for (int w = 0; w < currentHurtboxDefinition.hurtboxGroups[i].boxes.Count; w++)
                {
                    Pushbox hurtbox;
                    // Group doesn't already have a hurtbox, create one.
                    if (pushboxGroups[i].Count <= w)
                    {
                        hurtbox = CreateHurtbox();
                        pushboxGroups[i].Add(hurtbox);
                    }
                    else
                    {
                        // Group has a hurtbox here already.
                        hurtbox = pushboxGroups[i][w];
                    }
                    //hurtbox.Initialize(gameObject, currentHurtboxDefinition.hurtboxGroups[i]);
                    hurtbox.gameObject.SetActive(true);

                    // Set the hurtbox's position/rotation/etc.
                    SetHurtboxInfo(i, w);
                }
            }

            // CLEANUP EXTRA HURTBOXES //
            foreach (int k in pushboxGroups.Keys)
            {
                if (k >= currentHurtboxDefinition.hurtboxGroups.Count)
                {
                    hurtboxGroupsToDelete.Add(k);
                    CleanupHurtboxGroup(k);
                }
            }

            for (int h = 0; h < hurtboxGroupsToDelete.Count; h++)
            {
                pushboxGroups.Remove(h);
            }
            hurtboxGroupsToDelete.Clear();
        }

        protected virtual bool CheckWithinHurtboxWindow(StateHurtboxDefinition currentHurtboxDefinition, int groupIndex, uint frame)
        {
            if (currentHurtboxDefinition.hurtboxGroups[groupIndex].activeFramesEnd == -1)
            {
                return true;
            }
            if (frame > currentHurtboxDefinition.hurtboxGroups[groupIndex].activeFramesEnd
                || frame < currentHurtboxDefinition.hurtboxGroups[groupIndex].activeFramesStart)
            {
                return false;
            }
            return true;
        }

        protected virtual void CleanupHurtboxGroups()
        {
            foreach (int id in pushboxGroups.Keys)
            {
                for (int i = 0; i < pushboxGroups[id].Count; i++)
                {
                    DestroyHurtbox(pushboxGroups[id][i]);
                }
            }
            pushboxGroups.Clear();
        }

        protected virtual void CleanupHurtboxGroup(int groupID)
        {
            for (int i = 0; i < pushboxGroups[groupID].Count; i++)
            {
                DestroyHurtbox(pushboxGroups[groupID][i]);
            }
            pushboxGroups[groupID].Clear();
        }

        protected virtual void SetHurtboxInfo(int groupID, int hurtboxIndex)
        {
            BoxDefinition bd = (BoxDefinition)hurtboxDefinition.hurtboxGroups[groupID].boxes[hurtboxIndex];
            BoxCollider bc = pushboxGroups[groupID][hurtboxIndex].GetComponent<BoxCollider>();
            bc.size = bd.size;
            bc.transform.localPosition = bd.offset;
        }

        protected virtual Pushbox CreateHurtbox()
        {
            Pushbox pushbox;
            // Hurtbox in the pool.
            if (pushboxPool.Count > 0)
            {
                pushbox = pushboxPool[0];
                pushboxPool.RemoveAt(0);
            }
            else
            {
                pushbox = GameObject.Instantiate(pushboxPrefab, gameObject.transform, false);
            }
            pushbox.gameObject.SetActive(false);
            return pushbox;
        }

        /// <summary>
        /// Deactivates a hurtbox and returns it to the pool.
        /// </summary>
        /// <param name="pushbox">The hurtbox to deactivate.</param>
        protected virtual void DestroyHurtbox(Pushbox pushbox)
        {
            pushboxPool.Add(pushbox);
            pushbox.gameObject.SetActive(false);
        }

        public virtual void SetHurtboxDefinition(StateHurtboxDefinition stateHurtboxDefinition)
        {
            CleanupHurtboxGroups();
            hurtboxDefinition = stateHurtboxDefinition;
        }
    }
}