using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterState : HnSF.Fighters.FighterState
    {
        public FighterManager FighterManager { get { return Manager as FighterManager; } }
        public FighterStateManager StateManager { get { return Manager.StateManager as FighterStateManager; } }
        public FighterPhysicsManager PhysicsManager { get { return Manager.PhysicsManager as FighterPhysicsManager; } }
        public FighterInputManager InputManager { get { return Manager.InputManager as FighterInputManager; } }
        public FighterStatsManager Stats { get { return (Manager as FighterManager).StatsManager; } }
    }
}