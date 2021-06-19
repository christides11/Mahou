using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public static class SimulationDeletionManager
    {
        public static List<DeletionRequest> deletionRequests = new List<DeletionRequest>();

        public static void RequestDeletion(ISimObject simObject)
        {
            simObject.Disable();
            for(int i = 0; i < deletionRequests.Count; i++)
            {
                if(deletionRequests[i].simObject == simObject)
                {
                    return;
                }
            }
            deletionRequests.Add(new DeletionRequest() { frameRequested = SimulationManagerBase.instance.CurrentTick, simObject = simObject });
        }

        public static void Cleanup()
        {
            for(int i = deletionRequests.Count-1; i >= 0; i--)
            {
                if(deletionRequests[i].frameRequested > (SimulationManagerBase.instance as ServerSimulationManager).GetEarliestConfirmedClientTick())
                {
                    if(deletionRequests[i].simObject.ObjectEnabled == false)
                    {

                    }
                    else
                    {
                        deletionRequests.RemoveAt(i);
                    }
                }
            }
        }

        public struct DeletionRequest
        {
            public int frameRequested;
            public ISimObject simObject;
        }
    }
}