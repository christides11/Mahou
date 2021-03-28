using Mirror;

namespace Mahou.Networking
{
    public struct MatchInitMessage : NetworkMessage
    {
        public uint worldTick;

        public MatchInitMessage(uint worldTick)
        {
            this.worldTick = worldTick;
        }
    }
}
