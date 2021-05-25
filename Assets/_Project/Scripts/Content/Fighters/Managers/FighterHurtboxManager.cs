using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterHurtboxManager : HnSF.Fighters.FighterHurtboxManager
    {
        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
        }
    }
}