using Mahou.Input;
using System.Collections.Generic;

namespace Mahou.Simulation
{
    [System.Serializable]
    public class WorldSnapshot
    {
        /// <summary>
        /// The tick that this snapshot is for.
        /// </summary>
        public int currentTick;

        public List<ClientSimState> clientStates;
    }
}