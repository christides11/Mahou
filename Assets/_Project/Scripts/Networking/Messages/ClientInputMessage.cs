using Mirror;
using Mahou.Input;

namespace Mahou.Networking
{
    public struct ClientInputMessage : NetworkMessage
    {
        // The world tick for the first input in the array.
        public int StartWorldTick;

        // An array of inputs, one entry for tick.  Ticks are guaranteed to be contiguous.
        public ClientInput[] Inputs;

        // For each input:
        // Delta between the input world tick and the tick the server was at for that input.
        // TODO: This may be overkill, determining an average is probably better, but for now
        // this will give us 100% accuracy over lag compensation.
        public short[] ClientWorldTickDeltas;
    }
}
