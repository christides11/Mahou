using HnSF.Combat;
using Mahou.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    public class HitboxManager : HnSF.Combat.HitboxManager
    {
        public LayerMask hitboxLayerMask;

        Collider[] raycastHitList = new Collider[3];
        protected override void CheckBoxCollision(HitboxGroup hitboxGroup, int boxIndex)
        {
            Vector3 position = transform.position;
            Vector3 size = (hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).size;

            int cldAmt = 0;
            switch (hitboxGroup.boxes[boxIndex].shape)
            {
                case BoxShape.Rectangle:
                    cldAmt = Physics.OverlapBoxNonAlloc(position, size/2.0f, raycastHitList,
                        Quaternion.Euler((hitboxGroup.boxes[boxIndex] as HnSF.Combat.BoxDefinition).rotation), hitboxLayerMask);
                    break;
            }

            if (hurtboxes.Count < raycastHitList.Length)
            {
                hurtboxes.AddRange(new Hurtbox[raycastHitList.Length - hurtboxes.Count]);
            }

            for (int i = 0; i < cldAmt; i++)
            {
                Hurtbox h = raycastHitList[i].GetComponent<Hurtbox>();
                hurtboxes[i] = h;
            }
        }

        protected override HurtInfoBase BuildHurtInfo(HitboxGroup hitboxGroup, int hitboxIndex, HnSF.Combat.Hurtbox hurtbox)
        {
            HurtInfo hurtInfo;
            hurtInfo = new HurtInfo((Combat.HitInfo)hitboxGroup.hitboxHitInfo, hurtbox.HurtboxGroup as Mahou.Combat.HurtboxGroup,
                transform.position, transform.forward, transform.right, Vector3.zero);
            return hurtInfo;
        }
    }
}
