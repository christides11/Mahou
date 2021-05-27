using HnSF.Combat;
using Mahou.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterHitboxManager : HnSF.Fighters.FighterHitboxManager
    {
        public Vector3 referencePosition;

        public virtual void Initialize()
        {
            combatManager = GetComponent<FighterCombatManager>();
            manager = GetComponent<FighterManager>();
        }

        public override void Reset()
        {
            base.Reset();
            referencePosition = Vector3.zero;
        }

        protected override bool ShouldHurt(HitboxGroup hitboxGroup, int hitboxIndex, HnSF.Combat.Hurtbox hurtbox)
        {
            if (hurtbox.Owner.TryGetComponent(out IHurtable ih))
            {
                Combat.TeamTypes team = (Combat.TeamTypes)ih.GetTeam();
                if (team == Combat.TeamTypes.FFA)
                {
                    // Entity is in the FFA team.
                    return true;
                }
                else if (team != (Combat.TeamTypes)combatManager.GetTeam())
                {
                    // Entity is not in our team.
                    return true;
                }
                // Entity is on our team.
                return false;
            }
            // Not hurtable. Ignore.
            return false;
        }

        Collider[] raycastHitList = new Collider[3];
        protected override HnSF.Combat.Hurtbox[] CheckBoxCollision(HitboxGroup hitboxGroup, int boxIndex)
        {
            FighterManager fm = manager as FighterManager;
            
            Vector3 modifiedOffset = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).offset;
            modifiedOffset = modifiedOffset.x * fm.visual.transform.right
                + modifiedOffset.z * fm.visual.transform.forward
                + modifiedOffset.y * Vector3.up;
            Vector3 position = hitboxGroup.attachToEntity ? manager.visual.transform.position + modifiedOffset
                : referencePosition + modifiedOffset;
            Vector3 size = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).size;

            int cldAmt = 0;
            switch (hitboxGroup.boxes[boxIndex].shape)
            {
                case BoxShape.Rectangle:
                    cldAmt = Physics.OverlapBoxNonAlloc(position, size, raycastHitList,
                        Quaternion.Euler((hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).rotation),
                        combatManager.hitboxLayerMask);
                    //ExtDebug.DrawBox(position, size, Quaternion.Euler((hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).rotation), Color.red, 1);
                    break;
            }

            Hurtbox[] hurtboxes = new Hurtbox[cldAmt];
            for (int i = 0; i < cldAmt; i++)
            {
                Hurtbox h = raycastHitList[i].GetComponent<Hurtbox>();
                if (h.Owner != manager.gameObject)
                {
                    hurtboxes[i] = h;
                }
            }
            return hurtboxes;
        }

        protected override HurtInfoBase BuildHurtInfo(HitboxGroup hitboxGroup, int hitboxIndex, HnSF.Combat.Hurtbox hurtbox)
        {
            HurtInfo hurtInfo;
            //FighterManager fm = manager as FighterManager;
            switch (hitboxGroup.hitboxHitInfo.forceRelation)
            {
                case HitboxForceRelation.ATTACKER:
                    hurtInfo = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, transform.position, manager.visual.transform.forward,
                        manager.visual.transform.right);
                    break;
                case HitboxForceRelation.HITBOX:
                    Vector3 position = hitboxGroup.attachToEntity ? manager.transform.position + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset
                : referencePosition + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset;
                    hurtInfo = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, position, manager.visual.transform.forward,
                        manager.visual.transform.right);
                    break;
                case HitboxForceRelation.WORLD:
                    hurtInfo = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, transform.position, Vector3.forward,
                        Vector3.right);
                    break;
                default:
                    hurtInfo = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, transform.position, manager.visual.transform.forward,
                        manager.visual.transform.right);
                    break;
            }
            return hurtInfo;
        }
    }
}