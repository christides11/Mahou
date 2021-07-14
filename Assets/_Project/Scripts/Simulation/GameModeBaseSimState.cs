using Mahou.Content;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public class GameModeBaseSimState
    {
        public GameModeState gameModeState;

        public virtual System.Guid GetGUID()
        {
            return StaticGetGUID();
        }

        public static System.Guid StaticGetGUID()
        {
            return new System.Guid("a82a1b79-0354-4d7a-a944-6f9f0cbd77dd");
        }
    }
}