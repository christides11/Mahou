using Mirror;

namespace Mahou.Networking
{
    public struct MatchInitMessage : NetworkMessage
    {
        public int worldTick;

        public MatchInitMessage(int worldTick)
        {
            this.worldTick = worldTick;
        }
    }
}
