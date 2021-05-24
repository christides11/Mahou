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
            if (!modLoader.loadedMods.TryGetValue(map.modIdentifier, out LoadedModDefinition mod))
            {
                return false;
            }

            IMapDefinition sd = (IMapDefinition)mod.definition.GetContentDefinition(ContentType.Map, map.objectIdentifier);
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
            foreach(string m in modLoader.loadedMods.Keys)
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
            if (!modLoader.loadedMods.ContainsKey(modIdentifier))
            {
                return false;
            }
            return await modLoader.loadedMods[modIdentifier].definition.LoadContentDefinitions(contentType);
        }

        public async UniTask<bool> LoadContentDefinition(ContentType contentType, ModObjectReference objectReference)
        {
            if (!modLoader.loadedMods.ContainsKey(objectReference.modIdentifier))
            {
                return false;
            }
            return await modLoader.loadedMods[objectReference.modIdentifier].definition.LoadContentDefinition(contentType, objectReference.objectIdentifier);
        }

        public List<ModObjectReference> GetContentDefinitionReferences(ContentType contentType)
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            foreach (string m in modLoader.loadedMods.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences(contentType, m));
            }
            return content;
        }

        public List<ModObjectReference> GetContentDefinitionReferences(ContentType contentType, string modIdentifier)
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            // Mod does not exist.
            if (!modLoader.loadedMods.ContainsKey(modIdentifier))
            {
                return content;
            }
            List<IContentDefinition> fds = modLoader.loadedMods[modIdentifier].definition.GetContentDefinitions(contentType);
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
            if (!modLoader.loadedMods.ContainsKey(reference.modIdentifier))
            {
                return null;
            }
             
            IContentDefinition g = modLoader.loadedMods[reference.modIdentifier].definition.GetContentDefinition(contentType, reference.objectIdentifier);
            return g;
        }

        public void UnloadContentDefinitions(ContentType contentType)
        {
            foreach(string m in modLoader.loadedMods.Keys)
            {
                UnloadContentDefinitions(contentType, m);
            }
        }

        public void UnloadContentDefinitions(ContentType contentType, string modIdentifier)
        {
            if (!modLoader.loadedMods.ContainsKey(modIdentifier))
            {
                return;
            }
            modLoader.loadedMods[modIdentifier].definition.UnloadContentDefinitions(contentType);
        }
        #endregion
    }
}