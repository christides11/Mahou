using HnSF.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.Conditions
{
    public class HurtboxHitCount : HnSF.AttackCondition
    {
        public int hurtboxIndex = 0;
        public int hitCount = 1;

        public override bool Result(FighterBase manager)
        {
            Mahou.Content.Fighters.FighterHurtboxManager hurtboxManager = (Content.Fighters.FighterHurtboxManager)manager.HurtboxManager;
            if(hurtboxManager.hurtboxHitCount.TryGetValue(hurtboxIndex, out int hurtboxHitCount))
            {
                if(hurtboxHitCount >= hitCount)
                {
                    return true;
                }
            }
            return false;
        }
    }
}