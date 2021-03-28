using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Input
{
    public struct ClientInput
    {
        public List<PlayerInput> playerInputs;

        public ClientInput(List<PlayerInput> playerInputs)
        {
            this.playerInputs = playerInputs;
        }

        public static bool IsDifferent(ClientInput source, ClientInput other)
        {
            if(source.playerInputs == null || other.playerInputs == null)
            {
                return true;
            }
            if(source.playerInputs.Count != other.playerInputs.Count)
            {
                return true;
            }
            for(int i = 0; i < other.playerInputs.Count; i++)
            {
                if(other.playerInputs[i].movement != source.playerInputs[i].movement)
                {
                    return true;
                }
            }
            return false;
        }
    }
}