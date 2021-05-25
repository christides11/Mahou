using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateManager : HnSF.Fighters.FighterStateManager
    {
        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
        }
    }
}