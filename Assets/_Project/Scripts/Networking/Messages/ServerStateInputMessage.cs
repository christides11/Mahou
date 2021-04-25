using Mirror;
using Mahou.Simulation;
using System.Collections.Generic;

namespace Mahou.Networking
{
    public struct ServerStateInputMessage : NetworkMessage
    {
        public int serverTick;
        public List<TickInput> clientInputs;

        public ServerStateInputMessage(int serverTick, List<TickInput> clientInputs)
        {
            this.serverTick = serverTick;
            this.clientInputs = clientInputs;
        }
    }
}