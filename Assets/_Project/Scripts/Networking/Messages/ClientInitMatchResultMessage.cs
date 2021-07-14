using Mirror;

namespace Mahou.Networking
{
    public struct ClientInitMatchResultMessage : NetworkMessage
    {
        public enum InitMatchResult
        {
            SUCCESS = 0,
            FAILED = 1
        }

        public InitMatchResult result;

        public ClientInitMatchResultMessage(InitMatchResult result)
        {
            this.result = result;
        }
    }
}
