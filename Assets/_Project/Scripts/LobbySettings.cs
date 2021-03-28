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

        public LobbySettings(ModObjectReference selectedGamemode, ModObjectReference selectedMap)
        {
            this.selectedMap = selectedMap;
            this.selectedGamemode = selectedGamemode;
        }
    }
}