using Cysharp.Threading.Tasks;
using Mahou.Content.Fighters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesModDefinition", menuName = "Mahou/Content/Addressables/ModDefinition")]
    public class AddressablesModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class IdentifierAssetReferenceRelation
        {
            public string identifier;
            public AssetReference asset;
        }

        public string Description { get { return description; } }

        public bool GamemodeDefinitionsLoaded { get { return gamemodeDefinitions.Count > 0 ? true : false; } }

        public bool MapDefinitionsLoaded { get { return mapDefinitions.Count > 0 ? true : false; } }
        public bool FighterDefinitionsLoaded { get { return fighterDefinitions.Count > 0 ? true : false; } }
        public bool BattleDefinitionsLoaded { get { return battleDefinitions.Count > 0 ? true : false; } }

        [TextArea] [SerializeField] private string description;
        [SerializeField] private List<AssetReference> fighterRefs = new List<AssetReference>();
        [SerializeField] private List<AssetReference> gamemodeRefs = new List<AssetReference>();
        [SerializeField] private List<AssetReference> mapRefs = new List<AssetReference>();
        [SerializeField] private List<AssetReference> battleRefs = new List<AssetReference>();

        [NonSerialized]
        private Dictionary<IFighterDefinition, AsyncOperationHandle> fighterDefinitions
            = new Dictionary<IFighterDefinition, AsyncOperationHandle>();

        [NonSerialized]
        private Dictionary<IGameModeDefinition, AsyncOperationHandle> gamemodeDefinitions
            = new Dictionary<IGameModeDefinition, AsyncOperationHandle>();

        [NonSerialized]
        private Dictionary<IMapDefinition, AsyncOperationHandle> mapDefinitions
            = new Dictionary<IMapDefinition, AsyncOperationHandle>();

        [NonSerialized]
        private Dictionary<IBattleDefinition, AsyncOperationHandle> battleDefinitions
            = new Dictionary<IBattleDefinition, AsyncOperationHandle>();

        private async Task<List<AsyncOperationHandle>> LoadRealData<T>(List<AssetReference> refs, Action<T> callback)
        {
            List<AsyncOperationHandle> data = new List<AsyncOperationHandle>();
            for (int i = 0; i < refs.Count; i++)
            {
                AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(refs[i]);
                await handle.Task;
                data.Add(handle);
            }
            return data;
        }

        #region Fighter
        public async UniTask LoadFighterDefinitions()
        {
            if (FighterDefinitionsLoaded)
            {
                return;
            }
            List<AsyncOperationHandle> result = await LoadRealData<IFighterDefinition>(fighterRefs, null);
            foreach (AsyncOperationHandle handle in result)
            {
                fighterDefinitions.Add(handle.Result as IFighterDefinition, handle);
            }
        }

        public List<IFighterDefinition> GetFighterDefinitions()
        {
            if (!FighterDefinitionsLoaded)
            {
                return null;
            }
            return fighterDefinitions.Keys.ToList();
        }

        IFighterDefinition IModDefinition.GetFighterDefinition(string fighterIdentifier)
        {
            if (!FighterDefinitionsLoaded)
            {
                return null;
            }

            var result = fighterDefinitions.Keys.FirstOrDefault(x => x.Identifier == fighterIdentifier);
            if (result == null)
            {
                return null;
            }
            return result;
        }

        public void UnloadFighterDefinitions()
        {
            List<AsyncOperationHandle> handlesToUnload = new List<AsyncOperationHandle>();
            foreach (var v in fighterDefinitions)
            {
                handlesToUnload.Add(v.Value);
            }
            for (int i = 0; i < handlesToUnload.Count; i++)
            {
                Addressables.Release(handlesToUnload[i]);
            }
            fighterDefinitions.Clear();
        }
        #endregion

        #region Gamemodes
        public async UniTask<bool> LoadGamemodeDefinitions()
        {
            if (GamemodeDefinitionsLoaded)
            {
                return true;
            }
            try
            {
                List<AsyncOperationHandle> result = await LoadRealData<IGameModeDefinition>(gamemodeRefs, null);
                foreach (AsyncOperationHandle handle in result)
                {
                    gamemodeDefinitions.Add(handle.Result as IGameModeDefinition, handle);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IGameModeDefinition GetGamemodeDefinition(string gamemodeIdentifier)
        {
            if (!GamemodeDefinitionsLoaded)
            {
                return null;
            }

            var result = gamemodeDefinitions.Keys.FirstOrDefault(x => x.Identifier == gamemodeIdentifier);
            if (result == null)
            {
                return null;
            }
            return result;
        }

        public List<IGameModeDefinition> GetGamemodeDefinitions()
        {
            if (!GamemodeDefinitionsLoaded)
            {
                return null;
            }
            return gamemodeDefinitions.Keys.ToList();
        }

        public void UnloadGamemodeDefinitions()
        {
            List<AsyncOperationHandle> handlesToUnload = new List<AsyncOperationHandle>();
            foreach (var v in gamemodeDefinitions)
            {
                handlesToUnload.Add(v.Value);
            }
            for (int i = 0; i < handlesToUnload.Count; i++)
            {
                Addressables.Release(handlesToUnload[i]);
            }
            gamemodeDefinitions.Clear();
        }
        #endregion

        #region Map
        public async UniTask LoadMapDefinitions()
        {
            if (MapDefinitionsLoaded)
            {
                return;
            }
            List<AsyncOperationHandle> result = await LoadRealData<IMapDefinition>(mapRefs, null);
            foreach (AsyncOperationHandle handle in result)
            {
                mapDefinitions.Add(handle.Result as IMapDefinition, handle);
            }
        }

        public List<IMapDefinition> GetMapDefinitions()
        {
            if (!MapDefinitionsLoaded)
            {
                return null;
            }
            return mapDefinitions.Keys.ToList();
        }

        public IMapDefinition GetMapDefinition(string mapIdentifier)
        {
            if (!MapDefinitionsLoaded)
            {
                return null;
            }
            var result = mapDefinitions.Keys.FirstOrDefault(x => x.Identifier == mapIdentifier);
            if (result == null)
            {
                return null;
            }
            return result;
        }

        public void UnloadMapDefinitions()
        {
            List<AsyncOperationHandle> handlesToUnload = new List<AsyncOperationHandle>();
            foreach (var v in mapDefinitions)
            {
                handlesToUnload.Add(v.Value);
            }
            for (int i = 0; i < handlesToUnload.Count; i++)
            {
                Addressables.Release(handlesToUnload[i]);
            }
            mapDefinitions.Clear();
        }
        #endregion

        #region Battle
        public async UniTask LoadBattleDefinitions()
        {
            if (BattleDefinitionsLoaded)
            {
                return;
            }
            List<AsyncOperationHandle> result = await LoadRealData<IBattleDefinition>(battleRefs, null);
            foreach (AsyncOperationHandle handle in result)
            {
                battleDefinitions.Add(handle.Result as IBattleDefinition, handle);
            }
        }

        public List<IBattleDefinition> GetBattleDefinitions()
        {
            if (BattleDefinitionsLoaded == false)
            {
                return null;
            }
            return battleDefinitions.Keys.ToList();
        }

        public IBattleDefinition GetBattleDefintion(string battleIdentifier)
        {
            if (BattleDefinitionsLoaded == false)
            {
                return null;
            }
            var result = battleDefinitions.Keys.FirstOrDefault(x => x.Identifier == battleIdentifier);
            if (result == null)
            {
                return null;
            }
            return result;
        }

        public void UnloadBattleDefinitions()
        {
            List<AsyncOperationHandle> handlesToUnload = new List<AsyncOperationHandle>();
            foreach (var v in battleDefinitions)
            {
                handlesToUnload.Add(v.Value);
            }
            for (int i = 0; i < handlesToUnload.Count; i++)
            {
                Addressables.Release(handlesToUnload[i]);
            }
            battleDefinitions.Clear();
        }

        public IBattleDefinition GetBattleDefinition(string battleIdentifier)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}