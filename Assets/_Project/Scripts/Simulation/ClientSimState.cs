using Mahou.Input;
using Mirror;
using System.Collections.Generic;

namespace Mahou.Simulation
{
    /// <summary>
    /// Keeps a list of the current state of all local players for this client.
    /// </summary>
    [System.Serializable]
    public struct ClientSimState : ISimState
    {
        public NetworkIdentity networkIdentity;

        public List<PlayerSimState> playersStates;

        public ClientSimState(NetworkIdentity networkIdentity, List<PlayerSimState> playersStates)
        {
            this.networkIdentity = networkIdentity;
            this.playersStates = playersStates;
        }
    }
}