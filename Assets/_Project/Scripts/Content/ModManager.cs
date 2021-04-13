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

            await LoadFighterDefinitions(arg2.fighterReference.modIdentifier);
            IFighterDefinition fighter = GetFighterDefinition(arg2.fighterReference);
            if(fighter == null)
            {
                NetworkClient.Send(new LoadFighterRequestMessage()
                {
                    requestID = arg2.requestID,
                    fighterReference = arg2.fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.FAILED
                });
                return;
            }
            await fighter.LoadFighter();
            NetworkClient.RegisterPrefab(fighter.GetFighter());
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

            IMapDefinition sd = mod.GetMapDefinition(map.objectIdentifier);
            if (sd == null)
            {
                return false;
            }

            await sd.LoadScene();
            return true;
        }

        #region Fighter
        public async UniTask<bool> LoadFighterDefinitions()
        {
            foreach(string m in mods.Keys)
            {
                bool result = await LoadFighterDefinitions(m);
                if(result == false)
                {
                    return false;
                }
            }
            return true;
        }

        public async UniTask<bool> LoadFighterDefinitions(string modIdentifier)
        {
            if (!mods.ContainsKey(modIdentifier))
            {
                return false;
            }

            await mods[modIdentifier].LoadFighterDefinitions();
            return true;
        }

        public IFighterDefinition GetFighterDefinition(ModObjectReference fighter)
        {
            if (!mods.ContainsKey(fighter.modIdentifier))
            {
                return null;
            }

            IFighterDefinition f = mods[fighter.modIdentifier].GetFighterDefinition(fighter.objectIdentifier);

            if (f == null)
            {
                return null;
            }
            return f;
        }

        public List<ModObjectReference> GetFighterDefinitions()
        {
            List<ModObjectReference> fighters = new List<ModObjectReference>();
            foreach (string m in mods.Keys)
            {
                fighters.InsertRange(fighters.Count, GetFighterDefinitions(m));
            }
            return fighters;
        }

        public List<ModObjectReference> GetFighterDefinitions(string modIdentifier)
        {
            List<ModObjectReference> fighters = new List<ModObjectReference>();
            if (!mods.ContainsKey(modIdentifier))
            {
                return fighters;
            }
            if (!mods[modIdentifier].FighterDefinitionsLoaded)
            {
                return fighters;
            }
            List<IFighterDefinition> fds = mods[modIdentifier].GetFighterDefinitions();
            if(fds == null)
            {
                return fighters;
            }
            foreach(IFighterDefinition fd in fds)
            {
                fighters.Add(new ModObjectReference(modIdentifier, fd.Identifier));
            }
            return fighters;
        }
        #endregion

        #region Gamemode
        public async UniTask<bool> LoadGamemodeDefinitions()
        {
            foreach (string m in mods.Keys)
            {
                bool result = await LoadGamemodeDefinitions(m);
                if (result == false)
                {
                    return false;
                }
            }
            return true;
        }

        public async UniTask<bool> LoadGamemodeDefinitions(string modIdentifier)
        {
            if (!mods.ContainsKey(modIdentifier))
            {
                return false;
            }

            return await mods[modIdentifier].LoadGamemodeDefinitions();
        }

        public IGameModeDefinition GetGamemodeDefinition(ModObjectReference gamemode)
        {
            if (!mods.ContainsKey(gamemode.modIdentifier))
            {
                return null;
            }

            IGameModeDefinition g = mods[gamemode.modIdentifier].GetGamemodeDefinition(gamemode.objectIdentifier);

            if (g == null)
            {
                return null;
            }

            return g;
        }

        public List<ModObjectReference> GetGamemodeDefinitions()
        {
            List<ModObjectReference> gamemodes = new List<ModObjectReference>();
            foreach (string m in mods.Keys)
            {
                gamemodes.InsertRange(gamemodes.Count, GetGamemodeDefinitions(m));
            }
            return gamemodes;
        }

        public List<ModObjectReference> GetGamemodeDefinitions(string modIdentifier)
        {
            List<ModObjectReference> gamemodes = new List<ModObjectReference>();
            if (!mods.ContainsKey(modIdentifier))
            {
                return gamemodes;
            }
            if (!mods[modIdentifier].GamemodeDefinitionsLoaded)
            {
                return gamemodes;
            }

            List<IGameModeDefinition> gmds = mods[modIdentifier].GetGamemodeDefinitions();
            if (gmds == null)
            {
                return gamemodes;
            }
            foreach (IGameModeDefinition gmd in gmds)
            {
                gamemodes.Add(new ModObjectReference(modIdentifier, gmd.Identifier));
            }
            return gamemodes;
        }

        public void UnloadGamemodeDefinitions()
        {
            foreach (string m in mods.Keys)
            {
                mods[m].UnloadGamemodeDefinitions();
            }
        }

        public void UnloadGamemodeDefinitions(string modIdentifier)
        {
            if (!mods.ContainsKey(modIdentifier))
            {
                return;
            }
            mods[modIdentifier].UnloadGamemodeDefinitions();
        }
        #endregion

        #region Map
        public async UniTask<bool> LoadMapDefinitions()
        {
            foreach (string m in mods.Keys)
            {
                bool result = await LoadMapDefinitions(m);
                if (result == false)
                {
                    return false;
                }
            }
            return true;
        }

        public async UniTask<bool> LoadMapDefinitions(string modIdentifier)
        {
            if (!mods.ContainsKey(modIdentifier))
            {
                return false;
            }

            await mods[modIdentifier].LoadMapDefinitions();
            return true;
        }

        public IMapDefinition GetMapDefinition(ModObjectReference map)
        {
            if (!mods.ContainsKey(map.modIdentifier))
            {
                return null;
            }

            IMapDefinition f = mods[map.modIdentifier].GetMapDefinition(map.objectIdentifier);

            if (f == null)
            {
                return null;
            }
            return f;
        }

        public List<ModObjectReference> GetMapDefinitions()
        {
            List<ModObjectReference> maps = new List<ModObjectReference>();
            foreach (string m in mods.Keys)
            {
                maps.InsertRange(maps.Count, GetMapDefinitions(m));
            }
            return maps;
        }

        public List<ModObjectReference> GetMapDefinitions(string modIdentifier)
        {
            List<ModObjectReference> maps = new List<ModObjectReference>();
            if (!mods.ContainsKey(modIdentifier))
            {
                return maps;
            }
            if (!mods[modIdentifier].MapDefinitionsLoaded)
            {
                return maps;
            }
            List<IMapDefinition> fds = mods[modIdentifier].GetMapDefinitions();
            if (fds == null)
            {
                return maps;
            }
            foreach (IMapDefinition md in fds)
            {
                maps.Add(new ModObjectReference(modIdentifier, md.Identifier));
            }
            return maps;
        }

        public void UnloadMapDefinitions()
        {
            foreach (string m in mods.Keys)
            {
                mods[m].UnloadMapDefinitions();
            }
        }

        public void UnloadMapDefinitions(string modIdentifier)
        {
            if (!mods.ContainsKey(modIdentifier))
            {
                return;
            }
            mods[modIdentifier].UnloadMapDefinitions();
        }
        #endregion
    }
}