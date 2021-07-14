using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Mahou.Networking
{
    public struct LobbyInfoMessage : NetworkMessage
    {
        public LobbySettings lobbySettings;
        public Dictionary<int, ClientLobbyInfo> clientInfo;

        public LobbyInfoMessage(LobbySettings settings, Dictionary<int, ClientLobbyInfo> clientInfo)
        {
            this.lobbySettings = settings;
            this.clientInfo = clientInfo;
        }
    }
}