using Mirror;
using Mahou.Input;
using UnityEngine;

namespace Mahou.Simulation
{
    public struct ServerConfirmDeletionMessage : NetworkMessage
    {
        public NetworkIdentity networkIdentity;
    }
}