using Mirror;
using Mahou.Simulation;

namespace Mahou.Networking
{
    public struct ServerWorldStateMessage : NetworkMessage
    {
        public WorldSnapshot worldSnapshot;
        public uint latestAckedInput;

        public ServerWorldStateMessage(WorldSnapshot worldSnapshot, uint latestAckedInput)
        {
            this.worldSnapshot = worldSnapshot;
            this.latestAckedInput = latestAckedInput;
        }
    }
}