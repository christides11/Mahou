using Mirror;

namespace Mahou.Networking
{
    public struct ServerStartMatchMessage : NetworkMessage
    {
        public int worldTick;

        public ServerStartMatchMessage(int worldTick)
        {
            this.worldTick = worldTick;
        }
    }
}
