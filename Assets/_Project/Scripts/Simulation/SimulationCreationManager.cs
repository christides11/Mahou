using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public static class SimulationCreationManager
    {
        public delegate void GameObjectReplacedAction(NetworkIdentity oldGameObject, NetworkIdentity newGameObject);
        public static event GameObjectReplacedAction OnGameObjectReplaced;

        public static List<CreationRequest> creationRequests = new List<CreationRequest>();

        public static void Initialize()
        {
            NetworkClient.RegisterHandler<ServerConfirmCreationMessage>(ClientConfirmCreation);
        }


        public static GameObject Create(AssetIdentifier gameObject, Vector3 position, Quaternion rotation)
        {
            if (NetworkServer.active)
            {
                return ServerCreation(gameObject, position, rotation);
            }
            else
            {
                return ClientCreation(gameObject, position, rotation);
            }
        }

        private static GameObject ServerCreation(AssetIdentifier projectile, Vector3 position, Quaternion rotation)
        {
            ServerSimulationManager ssm = (SimulationManagerBase.instance as ServerSimulationManager);
            GameObject resultObject = GameObject.Instantiate(projectile.gameObject, position, rotation);
            SimulationManagerBase.instance.RegisterSimulationObject(resultObject.GetComponent<NetworkIdentity>());
            NetworkServer.Spawn(resultObject, projectile.GetGUID());
            NetworkServer.SendToAll(new ServerConfirmCreationMessage()
            {
                frameRequested = SimulationManagerBase.instance.CurrentRealTick,
                guid = projectile.GetGUID(),
                position = position,
                realObject = resultObject.GetComponent<NetworkIdentity>()
            });
            return resultObject;
        }

        private static GameObject ClientCreation(AssetIdentifier projectile, Vector3 position, Quaternion rotation)
        {
            // Check if object was already created
            for (int i = creationRequests.Count - 1; i >= 0; i--)
            {
                if (creationRequests[i].frameRequested == SimulationManagerBase.instance.CurrentTick
                    && creationRequests[i].guid == projectile.GetGUID()
                    && Vector3.Distance(creationRequests[i].position, position) < 0.1f)
                {
                    return creationRequests[i].simObject.gameObject;
                }
            }
            // Create the object
            GameObject resultObject = GameObject.Instantiate(projectile.gameObject, position, rotation);
            SimulationManagerBase.instance.RegisterSimulationObject(resultObject.GetComponent<NetworkIdentity>());
            creationRequests.Add(new CreationRequest()
            {
                frameRequested = SimulationManagerBase.instance.CurrentTick,
                position = position,
                guid = projectile.GetGUID(),
                simObject = resultObject.GetComponent<NetworkIdentity>()
            });
            return resultObject;
        }

        private static void ClientConfirmCreation(ServerConfirmCreationMessage arg2)
        {
            for(int i = creationRequests.Count-1; i >= 0; i--)
            {
                if (creationRequests[i].frameRequested == arg2.frameRequested
                    && creationRequests[i].guid == arg2.guid
                    && Vector3.Distance(creationRequests[i].position, arg2.position) < 0.1f)
                {
                    SimulationManagerBase.instance.ReplaceSimulationObject(creationRequests[i].simObject, arg2.realObject);
                    OnGameObjectReplaced?.Invoke(creationRequests[i].simObject, arg2.realObject);
                    GameObject.Destroy(creationRequests[i].simObject.gameObject);
                    creationRequests.RemoveAt(i);
                    return;
                }
            }
            // If the object was not created locally, then we are probably behind the server
        }

        public struct CreationRequest
        {
            public int frameRequested;
            public Vector3 position;
            public System.Guid guid;
            public NetworkIdentity simObject;
        }
    }
}
