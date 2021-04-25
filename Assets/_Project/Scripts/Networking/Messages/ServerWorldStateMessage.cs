using Mirror;
using Mahou.Simulation;

namespace Mahou.Networking
{
    public struct ServerWorldStateMessage : NetworkMessage
    {
        public WorldSnapshot worldSnapshot;
        public int latestAckedInput;

        public ServerWorldStateMessage(WorldSnapshot worldSnapshot, int latestAckedInput)
        {
            this.worldSnapshot = worldSnapshot;
            this.latestAckedInput = latestAckedInput;
        }
    }
}