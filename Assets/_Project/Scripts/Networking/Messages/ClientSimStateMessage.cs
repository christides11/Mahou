using Mirror;
using Mahou.Input;

namespace Mahou.Simulation
{
    public struct ClientSimStateMessage : NetworkMessage
    {
        public int latestAckedServerWorldStateTick;
    }
}