using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public static class SimulationDeletionManager
    {
        public static List<DeletionRequest> deletionRequests = new List<DeletionRequest>();

        public static void Initialize()
        {
            NetworkClient.RegisterHandler<ServerConfirmDeletionMessage>(ClientConfirmDeletion);
        }

        private static void ClientConfirmDeletion(ServerConfirmDeletionMessage arg2)
        {
            if (NetworkServer.active)
            {
                return;
            }
            SimulationManagerBase.instance.UnregisterSimulationObject(arg2.networkIdentity);
        }

        public static void RequestDeletion(NetworkIdentity networkIdentity, ISimObject simObject)
        {
            if (NetworkServer.active)
            {
                ServerDeletion(networkIdentity, simObject);
            }
            else
            {
                ClientDeletion(simObject);
            }
        }

        private static void ServerDeletion(NetworkIdentity networkIdentity, ISimObject simObject)
        {
            simObject.Disable();
            for (int i = 0; i < deletionRequests.Count; i++)
            {
                if (deletionRequests[i].simObject == simObject)
                {
                    return;
                }
            }

            deletionRequests.Add(new DeletionRequest() { 
                networkIdentity = networkIdentity, 
                frameRequested = SimulationManagerBase.instance.CurrentTick, 
                simObject = simObject 
            });
        }

        private static void ClientDeletion(ISimObject simObject)
        {
            simObject.Disable();
        }

        public static void Cleanup()
        {
            for(int i = deletionRequests.Count-1; i >= 0; i--)
            {
                // Every client has confirmed past the frame where the deletion happens, meaning we can now actually delete the object.
                if((SimulationManagerBase.instance as ServerSimulationManager).GetEarliestConfirmedClientTick() > deletionRequests[i].frameRequested)
                {
                    NetworkServer.SendToAll(new ServerConfirmDeletionMessage()
                    {
                        networkIdentity = deletionRequests[i].networkIdentity
                    });
                    SimulationManagerBase.instance.UnregisterSimulationObject(deletionRequests[i].networkIdentity);
                    NetworkServer.Destroy(deletionRequests[i].networkIdentity.gameObject);
                    deletionRequests.RemoveAt(i);
                }
            }
        }

        public struct DeletionRequest
        {
            public int frameRequested;
            public NetworkIdentity networkIdentity;
            public ISimObject simObject;
        }
    }
}