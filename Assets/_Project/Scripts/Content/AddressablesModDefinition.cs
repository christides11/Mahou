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
        public class IdentifierAssetReferenceRelation<T> where T : UnityEngine.Object
        {
            public string identifier;
            public AssetReferenceT<T> asset;
        }

        public string Description { get { return description; } }

        [TextArea] [SerializeField] private string description;

        [SerializeField] private List<IdentifierAssetReferenceRelation<IFighterDefinition>> fighterRefs = new List<IdentifierAssetReferenceRelation<IFighterDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IGameModeDefinition>> gamemodeRefs = new List<IdentifierAssetReferenceRelation<IGameModeDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IMapDefinition>> mapRefs = new List<IdentifierAssetReferenceRelation<IMapDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IBattleDefinition>> battleRefs = new List<IdentifierAssetReferenceRelation<IBattleDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IFighterDefinition>> fighterReferences = new Dictionary<string, AssetReferenceT<IFighterDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IFighterDefinition>> fighterDefinitions
            = new Dictionary<string, OperationResult<IFighterDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IGameModeDefinition>> gamemodeReferences = new Dictionary<string, AssetReferenceT<IGameModeDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IGameModeDefinition>> gamemodeDefinitions
            = new Dictionary<string, OperationResult<IGameModeDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IMapDefinition>> mapReferences = new Dictionary<string, AssetReferenceT<IMapDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IMapDefinition>> mapDefinitions
            = new Dictionary<string, OperationResult<IMapDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IBattleDefinition>> battleReferences = new Dictionary<string, AssetReferenceT<IBattleDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IBattleDefinition>> battleDefinitions
            = new Dictionary<string, OperationResult<IBattleDefinition>>();

        public void OnEnable()
        {
            fighterReferences.Clear();
            gamemodeReferences.Clear();
            mapReferences.Clear();
            battleReferences.Clear();
            foreach (IdentifierAssetReferenceRelation<IFighterDefinition> a in fighterRefs)
            {
                fighterReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IGameModeDefinition> a in gamemodeRefs)
            {
                gamemodeReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IMapDefinition> a in mapRefs)
            {
                mapReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IBattleDefinition> a in battleRefs)
            {
                battleReferences.Add(a.identifier, a.asset);
            }
        }

        #region Fighter
        /// <summary>
        /// Loads all fighters contained in this mod.
        /// </summary>
        public async UniTask<bool> LoadFighterDefinitions()
        {
            // All fighters are already loaded.
            if (fighterDefinitions.Count == fighterReferences.Count)
            {
                return true;
            }
            try
            {
                foreach (string fighterIdentifier in fighterReferences.Keys)
                {
                    await LoadFighterDefinition(fighterIdentifier);
                }
                return true;
            } catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Loads the fighter by it's identifier.
        /// </summary>
        /// <param name="fighterIdentifier">The fighter to load.</param>
        /// <returns>True if the load was successful.</returns>
        public async UniTask<bool> LoadFighterDefinition(string fighterIdentifier)
        {
            // Fighter doesn't exist.
            if (fighterReferences.ContainsKey(fighterIdentifier) == false)
            {
                return false;
            }
            // Fighter already loaded.
            if (fighterDefinitions.ContainsKey(fighterIdentifier) == true)
            {
                return true;
            }
            OperationResult<IFighterDefinition> result
                = await AddressablesManager.LoadAssetAsync<IFighterDefinition>((AssetReferenceT<IFighterDefinition>)fighterReferences[fighterIdentifier]);
            if (result.Succeeded)
            {
                result.Value.Identifier = fighterIdentifier;
                fighterDefinitions.Add(fighterIdentifier, result);
                return true;
            }
            return false;
        }

        IFighterDefinition IModDefinition.GetFighterDefinition(string fighterIdentifier)
        {
            // Fighter does not exist, or was not loaded.
            if (fighterDefinitions.ContainsKey(fighterIdentifier) == false)
            {
                return null;
            }
            return fighterDefinitions[fighterIdentifier].Value;
        }

        public List<IFighterDefinition> GetFighterDefinitions()
        {
            List<IFighterDefinition> defs = new List<IFighterDefinition>();
            foreach (var fighter in fighterDefinitions.Values)
            {
                defs.Add(fighter.Value);
            }
            return defs;
        }

        /// <summary>
        /// Unload all fighters in this mod.
        /// </summary>
        public void UnloadFighterDefinitions()
        {
            foreach (var v in fighterDefinitions)
            {
                AddressablesManager.ReleaseAsset(fighterReferences[v.Key]);
            }
            fighterDefinitions.Clear();
        }


        /// <summary>
        /// Unload a fighter.
        /// </summary>
        /// <param name="fighterIdentifier">The fighter to unload.</param>
        public void UnloadFighterDefinition(string fighterIdentifier)
        {
            // Fighter is not loaded.
            if (fighterDefinitions.ContainsKey(fighterIdentifier) == false)
            {
                return;
            }
            AddressablesManager.ReleaseAsset(fighterReferences[fighterIdentifier]);
            fighterDefinitions.Remove(fighterIdentifier);
        }
        #endregion

        #region Gamemodes
        public async UniTask<bool> LoadGamemodeDefinitions()
        {
            // All fighters are already loaded.
            if (gamemodeDefinitions.Count == gamemodeReferences.Count)
            {
                return true;
            }
            try
            {
                foreach (string gamemodeIdentifier in gamemodeReferences.Keys)
                {
                    await LoadGamemodeDefinition(gamemodeIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public async UniTask<bool> LoadGamemodeDefinition(string gamemodeIdentifier)
        {
            // Gamemode doesn't exist.
            if (gamemodeReferences.ContainsKey(gamemodeIdentifier) == false)
            {
                return false;
            }
            // Gamemode already loaded.
            if (gamemodeDefinitions.ContainsKey(gamemodeIdentifier) == true)
            {
                return true;
            }
            OperationResult<IGameModeDefinition> result
                = await AddressablesManager.LoadAssetAsync<IGameModeDefinition>(gamemodeReferences[gamemodeIdentifier]);
            if (result.Succeeded)
            {
                result.Value.Identifier = gamemodeIdentifier;
                gamemodeDefinitions.Add(gamemodeIdentifier, result);
                return true;
            }
            return false;
        }

        public IGameModeDefinition GetGamemodeDefinition(string gamemodeIdentifier)
        {
            // Fighter does not exist, or was not loaded.
            if (gamemodeDefinitions.ContainsKey(gamemodeIdentifier) == false)
            {
                return null;
            }
            return gamemodeDefinitions[gamemodeIdentifier].Value;
        }

        public List<IGameModeDefinition> GetGamemodeDefinitions()
        {
            List<IGameModeDefinition> defs = new List<IGameModeDefinition>();
            foreach (var gamemode in gamemodeDefinitions.Values)
            {
                defs.Add(gamemode.Value);
            }
            return defs;
        }

        public void UnloadGamemodeDefinitions()
        {
            foreach (var v in gamemodeDefinitions)
            {
                AddressablesManager.ReleaseAsset(gamemodeReferences[v.Key]);
            }
            gamemodeDefinitions.Clear();
        }

        public void UnloadGamemodeDefinition(string gamemodeIdentifier)
        {
            // Fighter is not loaded.
            if (gamemodeDefinitions.ContainsKey(gamemodeIdentifier) == false)
            {
                return;
            }
            AddressablesManager.ReleaseAsset(gamemodeReferences[gamemodeIdentifier]);
            gamemodeDefinitions.Remove(gamemodeIdentifier);
        }
        #endregion

        #region Map
        public async UniTask<bool> LoadMapDefinitions()
        {
            // All fighters are already loaded.
            if (mapDefinitions.Count == mapReferences.Count)
            {
                return true;
            }
            try
            {
                foreach (string mapIdentifier in mapReferences.Keys)
                {
                    await LoadMapDefinition(mapIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public async UniTask<bool> LoadMapDefinition(string mapIdentifier)
        {
            // Map doesn't exist.
            if (mapReferences.ContainsKey(mapIdentifier) == false)
            {
                return false;
            }
            // Map already loaded.
            if (mapDefinitions.ContainsKey(mapIdentifier) == true)
            {
                return true;
            }
            OperationResult<IMapDefinition> result
                = await AddressablesManager.LoadAssetAsync<IMapDefinition>((AssetReferenceT<IMapDefinition>)mapReferences[mapIdentifier]);
            if (result.Succeeded)
            {
                result.Value.Identifier = mapIdentifier;
                mapDefinitions.Add(mapIdentifier, result);
                return true;
            }
            return false;
        }

        public List<IMapDefinition> GetMapDefinitions()
        {
            List<IMapDefinition> defs = new List<IMapDefinition>();
            foreach (var map in mapDefinitions.Values)
            {
                defs.Add(map.Value);
            }
            return defs;
        }

        public IMapDefinition GetMapDefinition(string mapIdentifier)
        {
            // Fighter does not exist, or was not loaded.
            if (mapDefinitions.ContainsKey(mapIdentifier) == false)
            {
                return null;
            }
            return mapDefinitions[mapIdentifier].Value;
        }

        public void UnloadMapDefinitions()
        {
            foreach (var v in mapDefinitions)
            {
                AddressablesManager.ReleaseAsset(mapReferences[v.Key]);
            }
            mapDefinitions.Clear();
        }

        public void UnloadMapDefinition(string mapIdentifier)
        {
            // Fighter is not loaded.
            if (mapDefinitions.ContainsKey(mapIdentifier) == false)
            {
                return;
            }
            AddressablesManager.ReleaseAsset(mapReferences[mapIdentifier]);
            mapDefinitions.Remove(mapIdentifier);
        }
        #endregion

        #region Battle
        public async UniTask<bool> LoadBattleDefinitions()
        {
            // All fighters are already loaded.
            if (battleDefinitions.Count == battleReferences.Count)
            {
                return true;
            }
            try
            {
                foreach (string battleIdentifier in battleReferences.Keys)
                {
                    await LoadBattleDefinition(battleIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public async UniTask<bool> LoadBattleDefinition(string battleIdentifier)
        {
            // Battle doesn't exist.
            if (battleReferences.ContainsKey(battleIdentifier) == false)
            {
                return false;
            }
            // Battle already loaded.
            if (battleDefinitions.ContainsKey(battleIdentifier) == true)
            {
                return true;
            }
            OperationResult<IBattleDefinition> result
                = await AddressablesManager.LoadAssetAsync<IBattleDefinition>(battleReferences[battleIdentifier]);
            if (result.Succeeded)
            {
                result.Value.Identifier = battleIdentifier;
                battleDefinitions.Add(battleIdentifier, result);
                return true;
            }
            return false;
        }

        public List<IBattleDefinition> GetBattleDefinitions()
        {
            List<IBattleDefinition> defs = new List<IBattleDefinition>();
            foreach (var battle in battleDefinitions.Values)
            {
                defs.Add(battle.Value);
            }
            return defs;
        }

        public IBattleDefinition GetBattleDefinition(string battleIdentifier)
        {
            // Fighter does not exist, or was not loaded.
            if (battleDefinitions.ContainsKey(battleIdentifier) == false)
            {
                return null;
            }
            return battleDefinitions[battleIdentifier].Value;
        }

        public void UnloadBattleDefinition(string battleIdentifier)
        {
            // Fighter is not loaded.
            if (battleDefinitions.ContainsKey(battleIdentifier) == false)
            {
                return;
            }
            AddressablesManager.ReleaseAsset(battleReferences[battleIdentifier]);
            battleDefinitions.Remove(battleIdentifier);
        }

        public void UnloadBattleDefinitions()
        {
            foreach (var v in battleDefinitions)
            {
                AddressablesManager.ReleaseAsset(battleReferences[v.Key]);
            }
            battleDefinitions.Clear();
        }
        #endregion
    }
}