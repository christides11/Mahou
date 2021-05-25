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

        protected override bool ShouldHurt(HitboxGroup hitboxGroup, int hitboxIndex, Hurtbox hurtbox)
        {
            /*
            if (hurtbox.Owner.TryGetComponent(out IHurtable ih))
            {
                TDAction.Combat.TeamTypes team = (TDAction.Combat.TeamTypes)ih.GetTeam();
                if (team == Combat.TeamTypes.FFA)
                {
                    // Enemy in free for all team, hurt them.
                    return true;
                }
                else if (team != (Combat.TeamTypes)combatManager.GetTeam())
                {
                    // Enemy is not in our team, hurt them.
                    return true;
                }
                // Enemy is on our team.
                return false;
            }
            // Not hurtable. Ignore.
            return false;*/
            return true;
        }

        Collider[] raycastHitList = new Collider[0];
        protected override Hurtbox[] CheckBoxCollision(HitboxGroup hitboxGroup, int boxIndex)
        {
            FighterManager fm = manager as FighterManager;

            
            Vector3 modifiedOffset = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).offset;
            /*modifiedOffset = new Vector3(modifiedOffset.x * fm.FaceDirection, modifiedOffset.y, 0);
            
            Vector2 position = hitboxGroup.attachToEntity ? (Vector2)manager.transform.position + (Vector2)modifiedOffset
                : (Vector2)referencePosition + (Vector2)modifiedOffset;
            Vector2 size = (Vector2)(hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).size;
            raycastHitList = Physics.OverlapBoxAll(position, size, 0, combatManager.hitboxLayerMask);

            Hurtbox[] hurtboxes = new Hurtbox[raycastHitList.Length];
            for (int i = 0; i < raycastHitList.Length; i++)
            {
                Hurtbox h = raycastHitList[i].GetComponent<Hurtbox>();
                if (h.Owner != manager.gameObject)
                {
                    hurtboxes[i] = h;
                }
            }
            return hurtboxes;*/
            return null;
        }

        protected override HurtInfoBase BuildHurtInfo(HitboxGroup hitboxGroup, int hitboxIndex, Hurtbox hurtbox)
        {
            HurtInfo hi2d;
            FighterManager fm = manager as FighterManager;
            switch (hitboxGroup.hitboxHitInfo.forceRelation)
            {
                case HitboxForceRelation.ATTACKER:
                    hi2d = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, transform.position, manager.visual.transform.forward,
                        manager.visual.transform.right);
                    break;
                case HitboxForceRelation.HITBOX:
                    Vector3 position = hitboxGroup.attachToEntity ? manager.transform.position + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset
                : referencePosition + (hitboxGroup.boxes[hitboxIndex] as HnSF.Combat.BoxDefinition).offset;
                    hi2d = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, position, manager.visual.transform.forward,
                        manager.visual.transform.right);
                    break;
                case HitboxForceRelation.WORLD:
                    hi2d = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, transform.position, Vector3.forward,
                        Vector3.right);
                    break;
                default:
                    hi2d = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, transform.position, manager.visual.transform.forward,
                        manager.visual.transform.right);
                    break;
            }
            return hi2d;
        }
    }
}