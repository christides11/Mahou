using Mahou.Content;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    [System.Serializable]
    public struct LobbySettings
    {
        public ModObjectReference selectedGamemode;
        public List<ModObjectReference> requiredContent;

        public LobbySettings(ModObjectReference selectedGamemode, List<ModObjectReference> requiredContent)
        {
            this.selectedGamemode = selectedGamemode;
            this.requiredContent = requiredContent;
        }
    }
}