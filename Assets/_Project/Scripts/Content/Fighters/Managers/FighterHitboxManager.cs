using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterHitboxManager : HnSF.Fighters.FighterHitboxManager
    {
        public virtual void Initialize()
        {
            combatManager = GetComponent<FighterCombatManager>();
            manager = GetComponent<FighterManager>();
        }
    }
}