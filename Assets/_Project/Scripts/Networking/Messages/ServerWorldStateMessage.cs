using Mirror;
using Mahou.Simulation;

namespace Mahou.Networking
{
    public struct ServerWorldStateMessage : NetworkMessage
    {
        public WorldSnapshot worldSnapshot;
        public int clientLatestAckedInput;

        public ServerWorldStateMessage(WorldSnapshot worldSnapshot, int latestAckedInput)
        {
            this.worldSnapshot = worldSnapshot;
            this.clientLatestAckedInput = latestAckedInput;
        }
    }
}