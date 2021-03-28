using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Mahou.Networking
{
    public struct LobbySettingsMessage : NetworkMessage
    {
        public LobbySettings lobbySettings;
    }
}