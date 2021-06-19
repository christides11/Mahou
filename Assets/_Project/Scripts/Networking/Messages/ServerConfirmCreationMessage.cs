using Mirror;
using Mahou.Input;
using UnityEngine;

namespace Mahou.Simulation
{
    public struct ServerConfirmCreationMessage : NetworkMessage
    {
        public int frameRequested;
        public System.Guid guid;
        public Vector3 position;
        public NetworkIdentity realObject;
    }
}