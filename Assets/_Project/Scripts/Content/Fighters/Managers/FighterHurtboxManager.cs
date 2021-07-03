using HnSF.Combat;
using Mahou.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoxDefinition = Mahou.Combat.BoxDefinition;

namespace Mahou.Content.Fighters
{
    public class FighterHurtboxManager : HnSF.Fighters.FighterHurtboxManager
    {
        [SerializeField] protected Hurtbox hurtboxPrefab;
        [SerializeField] protected Pushbox pushboxPrefab;

        public Dictionary<int, int> hurtboxHitCount = new Dictionary<int, int>();

        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
        }

        public virtual void Cleanup()
        {
            foreach (int id in hurtboxGroups.Keys)
            {
                for (int i = 0; i < hurtboxGroups[id].Count; i++)
                {
                    DestroyHurtbox(hurtboxGroups[id][i]);
                }
            }
            hurtboxGroups.Clear();
            hurtboxDefinition = null;
        }

        public virtual void Reset()
        {
            hurtboxHitCount.Clear();
        }

        public override void CreateHurtboxes(HnSF.Combat.StateHurtboxDefinition hurtboxDefinition, uint frame)
        {
            this.hurtboxDefinition = hurtboxDefinition;
            base.CreateHurtboxes(hurtboxDefinition, frame);
        }

        protected override void SetHurtboxInfo(int groupID, int hurtboxIndex)
        {
            BoxDefinition bd = (BoxDefinition)hurtboxDefinition.hurtboxGroups[groupID].boxes[hurtboxIndex];
            BoxCollider bc = hurtboxGroups[groupID][hurtboxIndex].GetComponent<BoxCollider>();
            bc.size = bd.size;
            bc.transform.localPosition = bd.offset;
        }

        protected override HnSF.Combat.Hurtbox CreateHurtbox()
        {
            Hurtbox hurtbox;
            // Hurtbox in the pool.
            if (hurtboxPool.Count > 0)
            {
                hurtbox = (Hurtbox)hurtboxPool[0];
                hurtboxPool.RemoveAt(0);
            }
            else
            {
                hurtbox = GameObject.Instantiate(hurtboxPrefab, gameObject.transform, false);
            }
            hurtbox.gameObject.SetActive(false);
            return hurtbox;
        }

        public StateHurtboxDefinition GetHurtboxDefinition()
        {
            return (StateHurtboxDefinition)hurtboxDefinition;
        }
    }
}