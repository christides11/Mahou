using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mahou.Content;

namespace Mahou.Networking
{
    public struct LoadFighterRequestMessage : NetworkMessage
    {
        public enum RequestType
        {
            INITREQUEST = 0,
            FAILED = 1,
            SUCCESS = 2
        }

        public int requestID;
        public RequestType requestType;
        public ModObjectReference fighterReference;
    }
}