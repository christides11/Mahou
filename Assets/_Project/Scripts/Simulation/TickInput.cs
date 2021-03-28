using Mirror;
using Mahou.Input;

namespace Mahou.Simulation
{
    public struct TickInput
    {
        public NetworkIdentity client;
        
        public uint currentServerTick;
        // The remote world tick the player saw other entities at for this input.
        // (This is equivalent to lastServerWorldTick on the client).
        public uint remoteViewTick;

        public ClientInput input;
    }
}