using Mirror;
using Mahou.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

namespace Mahou.Simulation
{
    public class ClientInputProcessor
    {
        /// <summary>
        /// PriorityQueue that keeps client inputs sent to the server.
        /// </summary>
        private SimplePriorityQueue<TickInput> queue = new SimplePriorityQueue<TickInput>();
        /// <summary>
        /// Keep the latest received input from each client ready to be checked.
        /// This is useful for when the server needs to reuse the latest input during rollback.
        /// </summary>
        private Dictionary<int, TickInput> latestPlayerInput = new Dictionary<int, TickInput>();

        /// <summary>
        /// Get the latest input for the given client, if they have sent any yet.
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool TryGetLatestInput(int clientID, out TickInput ret)
        {
            return latestPlayerInput.TryGetValue(clientID, out ret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldTick"></param>
        /// <returns></returns>
        public List<TickInput> DequeueInputsForTick(uint worldTick)
        {
            var ret = new List<TickInput>();
            TickInput entry;
            while (queue.TryDequeue(out entry))
            {
                if (entry.currentServerTick < worldTick)
                {

                }
                else if (entry.currentServerTick == worldTick)
                {
                    ret.Add(entry);
                }
                else
                {
                    // We dequeued a future input, put it back in.
                    queue.Enqueue(entry, entry.currentServerTick);
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cimsg">The input message the client sent to the server.</param>
        /// <param name="clientConn">The client's network connection.</param>
        /// <param name="serverCurrentTick">The current tick of the server simulation.</param>
        public void EnqueueInput(ClientInputMessage cimsg, NetworkConnection clientConn, uint serverCurrentTick)
        {
            // Calculate the last tick in the incoming command.
            uint maxTick = (uint)(cimsg.StartWorldTick + cimsg.Inputs.Length - 1);

            // If the message has inputs we haven't received yet, then we want to enqueue them.
            if (maxTick >= serverCurrentTick)
            {
                // If the server has already received inputs that are in this packet,
                // we want to ignore them.
                uint start = serverCurrentTick > cimsg.StartWorldTick
                    ? serverCurrentTick - cimsg.StartWorldTick : 0;

                // Apply those inputs.
                for (int i = (int)start; i < cimsg.Inputs.Length; ++i)
                {
                    uint inputWorldTick = cimsg.StartWorldTick + (uint)i;
                    // If we've already received an input for this tick, ignore the duplicate.
                    if(latestPlayerInput.ContainsKey(clientConn.connectionId)
                        && latestPlayerInput[clientConn.connectionId].currentServerTick >= inputWorldTick)
                    {
                        continue;
                    }

                    TickInput tickInput = new TickInput()
                    {
                        currentServerTick = inputWorldTick,
                        remoteViewTick = (uint)(inputWorldTick - cimsg.ClientWorldTickDeltas[i]),
                        client = clientConn.identity,
                        input = cimsg.Inputs[i]
                    };
                    queue.Enqueue(tickInput, inputWorldTick);

                    // Store the latest input in case the simulation needs to repeat missed frames.
                    latestPlayerInput[clientConn.connectionId] = tickInput;
                }
            }
            else
            {
                // ?
            }
        }
    }
}