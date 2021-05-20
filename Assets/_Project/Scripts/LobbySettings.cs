using Mahou.Content;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    public struct LobbySettings
    {
        public ModObjectReference selectedGamemode;
        public ModObjectReference selectedMap;
        public ModObjectReference selectedBattle;

        public LobbySettings(ModObjectReference selectedGamemode, ModObjectReference selectedMap, ModObjectReference selectedBattle)
        {
            this.selectedMap = selectedMap;
            this.selectedGamemode = selectedGamemode;
            this.selectedBattle = selectedBattle;
        }
    }
}