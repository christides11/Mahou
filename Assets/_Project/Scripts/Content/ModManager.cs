using Mahou.Content;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Mirror;
using Mahou.Networking;
using System;
using NetworkManager = Mahou.Networking.NetworkManager;

namespace Mahou.Managers
{
    public class ModManager : MonoBehaviour
    {
        public delegate void FighterRequestMsgAction(NetworkConnection c, LoadFighterRequestMessage msg);
        public static event FighterRequestMsgAction OnFighterRequestMsgResult;

        public static ModManager instance;

        public Dictionary<string, IModDefinition> mods = new Dictionary<string, IModDefinition>();

        public ModLoader ModLoader { get { return modLoader; } }

        [SerializeField] private GameManager gameManager;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private ModLoader modLoader;

        public void Initialize()
        {
            instance = this;
            modLoader.Init(this, gameManager);
            NetworkServer.RegisterHandler<LoadFighterRequestMessage>(ServerLoadFighterRequestHandler);
            NetworkClient.RegisterHandler<LoadFighterRequestMessage>(ClientLoadFighterRequestHandler);
        }

        private void ServerLoadFighterRequestHandler(NetworkConnection arg1, LoadFighterRequestMessage arg2)
        {
            OnFighterRequestMsgResult?.Invoke(arg1, arg2);
        }

        private async void ClientLoadFighterRequestHandler(LoadFighterRequestMessage arg2)
        {
            OnFighterRequestMsgResult?.Invoke(NetworkClient.connection, arg2);

            await LoadContentDefinition(ContentType.Fighter, arg2.fighterReference);
            IFighterDefinition fighterDefinition = (IFighterDefinition)GetContentDefinition(ContentType.Fighter, arg2.fighterReference);
            if(fighterDefinition == null)
            {
                NetworkClient.Send(new LoadFighterRequestMessage()
                {
                    requestID = arg2.requestID,
                    fighterReference = arg2.fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.FAILED
                });
                return;
            }
            if((await fighterDefinition.LoadFighter()) == false)
            {
                Debug.Log($"Failed loading fighter. {arg2.fighterReference}");
                NetworkClient.Send(new LoadFighterRequestMessage()
                {
                    requestID = arg2.requestID,
                    fighterReference = arg2.fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.FAILED
                });
                return;
            }
            GameObject fighterGameobject = fighterDefinition.GetFighter();
            if(fighterGameobject == null)
            {
                Debug.Log($"Failed getting fighter object. {arg2.fighterReference}");
                NetworkClient.Send(new LoadFighterRequestMessage()
                {
                    requestID = arg2.requestID,
                    fighterReference = arg2.fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.FAILED
                });
                return;
            }
            NetworkClient.RegisterPrefab(fighterGameobject, new System.Guid(fighterDefinition.GetFighterGUID()));
            NetworkClient.Send(new LoadFighterRequestMessage()
            {
                requestID = arg2.requestID,
                fighterReference = arg2.fighterReference,
                requestType = LoadFighterRequestMessage.RequestType.SUCCESS
            });
        }

        public async UniTask<bool> LoadMap(ModObjectReference map)
        {
            if (!mods.TryGetValue(map.modIdentifier, out IModDefinition mod))
            {
                return false;
            }

            IMapDefinition sd = (IMapDefinition)mod.GetContentDefinition(ContentType.Map, map.objectIdentifier);
            if (sd == null)
            {
                return false;
            }

            await sd.LoadMap();
            return true;
        }

        #region Content
        public async UniTask<bool> LoadContentDefinitions(ContentType contentType)
        {
            foreach(string m in mods.Keys)
            {
                bool result = await LoadContentDefinitions(contentType, m);
                if(result == false)
                {
                    return false;
                }
            }
            return true;
        }

        public async UniTask<bool> LoadContentDefinitions(ContentType contentType, string modIdentifier)
        {
            if (!mods.ContainsKey(modIdentifier))
            {
                return false;
            }
            return await mods[modIdentifier].LoadContentDefinitions(contentType);
        }

        public async UniTask<bool> LoadContentDefinition(ContentType contentType, ModObjectReference objectReference)
        {
            if (!mods.ContainsKey(objectReference.modIdentifier))
            {
                return false;
            }
            return await mods[objectReference.modIdentifier].LoadContentDefinition(contentType, objectReference.objectIdentifier);
        }

        public List<ModObjectReference> GetContentDefinitionReferences(ContentType contentType)
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            foreach (string m in mods.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences(contentType, m));
            }
            return content;
        }

        public List<ModObjectReference> GetContentDefinitionReferences(ContentType contentType, string modIdentifier)
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            // Mod does not exist.
            if (!mods.ContainsKey(modIdentifier))
            {
                return content;
            }
            List<IContentDefinition> fds = mods[modIdentifier].GetContentDefinitions(contentType);
            if (fds == null)
            {
                return content;
            }
            foreach (IContentDefinition fd in fds)
            {
                content.Add(new ModObjectReference(modIdentifier, fd.Identifier));
            }
            return content;
        }

        public IContentDefinition GetContentDefinition(ContentType contentType, ModObjectReference reference)
        {
            if (!mods.ContainsKey(reference.modIdentifier))
            {
                return null;
            }
             
            IContentDefinition g = mods[reference.modIdentifier].GetContentDefinition(contentType, reference.objectIdentifier);
            return g;
        }

        public void UnloadContentDefinitions(ContentType contentType)
        {
            foreach(string m in mods.Keys)
            {
                UnloadContentDefinitions(contentType, m);
            }
        }

        public void UnloadContentDefinitions(ContentType contentType, string modIdentifier)
        {
            if (!mods.ContainsKey(modIdentifier))
            {
                return;
            }
            mods[modIdentifier].UnloadContentDefinitions(contentType);
        }
        #endregion
    }
}