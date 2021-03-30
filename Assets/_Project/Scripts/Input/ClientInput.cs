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

        public static ClientInput Falloff(ClientInput input, int times, float falloff)
        {
            ClientInput cInput = new ClientInput(new List<PlayerInput>(input.playerInputs));

            for(int i = 0; i < times; i++)
            {
                for (int w = 0; w < cInput.playerInputs.Count; w++){
                    cInput.playerInputs[w] = new PlayerInput()
                    {
                        attack = cInput.playerInputs[w].attack,
                        jump = cInput.playerInputs[w].jump,
                        lockon = cInput.playerInputs[w].lockon,
                        movement = cInput.playerInputs[w].movement * falloff,
                        shoot = cInput.playerInputs[w].shoot
                    };
                }
            }

            return cInput;
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