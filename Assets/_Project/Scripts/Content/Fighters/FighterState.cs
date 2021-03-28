using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterState : CAF.Fighters.FighterState
    {
        public FighterStateManager StateManager { get { return Manager.StateManager as FighterStateManager; } }
    }
}