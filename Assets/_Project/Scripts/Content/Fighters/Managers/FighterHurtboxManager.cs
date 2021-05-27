using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterHurtboxManager : HnSF.Fighters.FighterHurtboxManager
    {
        [SerializeField] protected Hurtbox hurtboxPrefab;

        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
        }

        public virtual void Reset()
        {
            foreach (int id in hurtboxGroups.Keys)
            {
                for (int i = 0; i < hurtboxGroups[id].Count; i++)
                {
                    DestroyHurtbox(hurtboxGroups[id][i]);
                }
            }
            hurtboxGroups.Clear();
            currentHurtboxDefinition = null;
        }

        public virtual void CreateHurtboxes(StateHurtboxDefinition hurtboxDefinition, uint frame)
        {
            this.currentHurtboxDefinition = hurtboxDefinition;
            CreateHurtboxes(frame);
        }

        protected override void SetHurtboxInfo(int groupID, int hurtboxIndex)
        {
            BoxDefinition bd = (BoxDefinition)currentHurtboxDefinition.hurtboxGroups[groupID].boxes[hurtboxIndex];
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
                hurtbox.Owner = gameObject;
            }
            hurtbox.gameObject.SetActive(false);
            return hurtbox;
        }
    }
}