using Mahou.Content;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public class GameModeWaveBattleSimState : GameModeBaseSimState
    {
        public int initTimer;

        public override System.Guid GetGUID()
        {
            return StaticGetGUID();
        }

        public static new System.Guid StaticGetGUID()
        {
            return new System.Guid("a12fee28-1c68-444f-81e9-a4b42c0d8365");
        }
    }
}