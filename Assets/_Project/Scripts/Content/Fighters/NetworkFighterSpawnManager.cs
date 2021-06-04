using Cysharp.Threading.Tasks;
using Mahou.Content;
using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    public static class NetworkFighterSpawnManager
    {
        public delegate void FighterRequestMsgAction(NetworkConnection c, LoadFighterRequestMessage msg);
        public static event FighterRequestMsgAction OnFighterRequestMsgResult;

        public static int CurrentRequestNumber { get { return fighterRequestNumber; } }
        private static int fighterRequestNumber = 0;

        // fighterRequestNumber : List of unconfirmed clients
        private static Dictionary<int, List<NetworkConnection>> unconfirmedClients = new Dictionary<int, List<NetworkConnection>>();
        private static Dictionary<int, float> unconfirmedClientsTimeout = new Dictionary<int, float>();

        public static void Initialize()
        {
            NetworkServer.RegisterHandler<LoadFighterRequestMessage>(ServerLoadFighterRequestHandler);
            NetworkClient.RegisterHandler<LoadFighterRequestMessage>(ClientLoadFighterRequestHandler);
        }

        public static async UniTask<bool> ServerRequestFighterLoad(ModObjectReference fighterReference, float timeoutTime = 1.0f)
        {
            int currentRequestNumber = fighterRequestNumber;
            unconfirmedClients.Add(currentRequestNumber, new List<NetworkConnection>());
            unconfirmedClientsTimeout.Add(currentRequestNumber, timeoutTime);
            fighterRequestNumber++;

            if ((await ContentManager.instance.LoadContentDefinition(ContentType.Fighter, fighterReference)) == false)
            {
                Debug.Log($"SERVER: Failed loading {fighterReference} definition during request {currentRequestNumber}.");
                return false;
            }

            IFighterDefinition fighter = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, fighterReference);
            if ((await fighter.LoadFighter()) == false)
            {
                Debug.Log($"SERVER: Failed loading fighter during request {currentRequestNumber}.");
                return false;
            }

            // Tell all clients to try loading the fighter.
            foreach (var c in NetworkServer.connections)
            {
                // Ignore host.
                if (NetworkServer.localClientActive
                    && NetworkServer.localConnection.connectionId == c.Value.connectionId)
                {
                    continue;
                }
                c.Value.Send(new LoadFighterRequestMessage()
                {
                    requestID = currentRequestNumber,
                    fighterReference = fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.INITREQUEST
                });
                unconfirmedClients[currentRequestNumber].Add(c.Value);
            }

            while(unconfirmedClients[currentRequestNumber].Count > 0 && unconfirmedClientsTimeout[currentRequestNumber] > 0)
            {
                unconfirmedClientsTimeout[currentRequestNumber] -= Time.deltaTime;
                await UniTask.Yield();
            }
            unconfirmedClients.Remove(currentRequestNumber);
            unconfirmedClientsTimeout.Remove(currentRequestNumber);
            return true;
        }

        private static void ServerLoadFighterRequestHandler(NetworkConnection arg1, LoadFighterRequestMessage arg2)
        {
            if (unconfirmedClients.ContainsKey(arg2.requestID) == false)
            {
                return;
            }
            unconfirmedClients[arg2.requestID].Remove(arg1);
            switch (arg2.requestType)
            {
                case LoadFighterRequestMessage.RequestType.FAILED:
                    arg1.Disconnect();
                    break;
            }
        }

        private static async void ClientLoadFighterRequestHandler(LoadFighterRequestMessage arg2)
        {
            OnFighterRequestMsgResult?.Invoke(NetworkClient.connection, arg2);

            await ContentManager.instance.LoadContentDefinition(ContentType.Fighter, arg2.fighterReference);
            IFighterDefinition fighterDefinition = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, arg2.fighterReference);
            if (fighterDefinition == null)
            {
                NetworkClient.Send(new LoadFighterRequestMessage()
                {
                    requestID = arg2.requestID,
                    fighterReference = arg2.fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.FAILED
                });
                return;
            }
            if ((await fighterDefinition.LoadFighter()) == false)
            {
                Debug.Log($"CLIENT: Failed loading fighter definition. {arg2.fighterReference}");
                NetworkClient.Send(new LoadFighterRequestMessage()
                {
                    requestID = arg2.requestID,
                    fighterReference = arg2.fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.FAILED
                });
                return;
            }
            GameObject fighterGameobject = fighterDefinition.GetFighter();
            if (fighterGameobject == null)
            {
                Debug.Log($"CLIENT: Failed getting fighter object. {arg2.fighterReference}");
                NetworkClient.Send(new LoadFighterRequestMessage()
                {
                    requestID = arg2.requestID,
                    fighterReference = arg2.fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.FAILED
                });
                return;
            }

            System.Guid fighterGuid = new System.Guid(fighterDefinition.GetFighterGUID());
            if (NetworkClient.prefabs.ContainsKey(fighterGuid) == false)
            {
                NetworkClient.RegisterPrefab(fighterGameobject, fighterGuid);
            }

            NetworkClient.Send(new LoadFighterRequestMessage()
            {
                requestID = arg2.requestID,
                fighterReference = arg2.fighterReference,
                requestType = LoadFighterRequestMessage.RequestType.SUCCESS
            });
        }
    }
}