using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterCombatManager : HnSF.Fighters.FighterCombatManager
    {

        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
            hitboxManager = GetComponent<FighterHitboxManager>();
        }
    }
}