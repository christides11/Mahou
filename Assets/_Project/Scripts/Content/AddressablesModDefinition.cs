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
        [SerializeField] private List<IdentifierAssetReferenceRelation<IGameModeComponentDefinition>> gamemodeComponentRefs = new List<IdentifierAssetReferenceRelation<IGameModeComponentDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IMapDefinition>> mapRefs = new List<IdentifierAssetReferenceRelation<IMapDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<IBattleDefinition>> battleRefs = new List<IdentifierAssetReferenceRelation<IBattleDefinition>>();
        [SerializeField] private List<IdentifierAssetReferenceRelation<ISongDefinition>> songRefs = new List<IdentifierAssetReferenceRelation<ISongDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IFighterDefinition>> fighterReferences = new Dictionary<string, AssetReferenceT<IFighterDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IFighterDefinition>> fighterDefinitions
            = new Dictionary<string, OperationResult<IFighterDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IGameModeDefinition>> gamemodeReferences = new Dictionary<string, AssetReferenceT<IGameModeDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IGameModeDefinition>> gamemodeDefinitions
            = new Dictionary<string, OperationResult<IGameModeDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IGameModeComponentDefinition>> gamemodeComponentReferences = new Dictionary<string, AssetReferenceT<IGameModeComponentDefinition>>();
        [NonSerialized]
        private Dictionary<string, OperationResult<IGameModeComponentDefinition>> gamemodeComponentDefinitions
            = new Dictionary<string, OperationResult<IGameModeComponentDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IMapDefinition>> mapReferences = new Dictionary<string, AssetReferenceT<IMapDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IMapDefinition>> mapDefinitions
            = new Dictionary<string, OperationResult<IMapDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<IBattleDefinition>> battleReferences = new Dictionary<string, AssetReferenceT<IBattleDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<IBattleDefinition>> battleDefinitions
            = new Dictionary<string, OperationResult<IBattleDefinition>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<ISongDefinition>> songReferences = new Dictionary<string, AssetReferenceT<ISongDefinition>>();
        [NonSerialized] private Dictionary<string, OperationResult<ISongDefinition>> songDefinitions
            = new Dictionary<string, OperationResult<ISongDefinition>>();

        public void OnEnable()
        {
            fighterReferences.Clear();
            gamemodeReferences.Clear();
            gamemodeComponentReferences.Clear();
            mapReferences.Clear();
            battleReferences.Clear();
            songReferences.Clear();
            foreach (IdentifierAssetReferenceRelation<IFighterDefinition> a in fighterRefs)
            {
                fighterReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IGameModeDefinition> a in gamemodeRefs)
            {
                gamemodeReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IGameModeComponentDefinition> a in gamemodeComponentRefs)
            {
                gamemodeComponentReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IMapDefinition> a in mapRefs)
            {
                mapReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<IBattleDefinition> a in battleRefs)
            {
                battleReferences.Add(a.identifier, a.asset);
            }
            foreach (IdentifierAssetReferenceRelation<ISongDefinition> a in songRefs)
            {
                songReferences.Add(a.identifier, a.asset);
            }
        }

        #region Content
        public bool ContentExist(ContentType contentType, string contentIdentfier)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return fighterReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Scenario:
                    return battleReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Gamemode:
                    return gamemodeReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.GamemodeComponent:
                    return gamemodeComponentReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Map:
                    return mapReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Song:
                    return songReferences.ContainsKey(contentIdentfier) ? true : false;
                case ContentType.Battle:
                    return battleReferences.ContainsKey(contentIdentfier) ? true : false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async UniTask<bool> LoadContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Fighter:
                    return await LoadContentDefinitions(fighterReferences, fighterDefinitions);
                case ContentType.Scenario:
                    return await LoadContentDefinitions(battleReferences, battleDefinitions);
                case ContentType.Gamemode:
                    return await LoadContentDefinitions(gamemodeReferences, gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinitions(gamemodeComponentReferences, gamemodeComponentDefinitions);
                case ContentType.Map:
                    return await LoadContentDefinitions(mapReferences, mapDefinitions);
                case ContentType.Song:
                    return await LoadContentDefinitions(songReferences, songDefinitions);
                case ContentType.Battle:
                    return await LoadContentDefinitions(battleReferences, battleDefinitions);
            }
            return false;
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentIdentifier"></param>
        /// <returns></returns>
        public async UniTask<bool> LoadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Scenario:
                    return await LoadContentDefinition(battleReferences, battleDefinitions, contentIdentifier);
                case ContentType.Fighter:
                    return await LoadContentDefinition(fighterReferences, fighterDefinitions, contentIdentifier);
                case ContentType.Gamemode:
                    return await LoadContentDefinition(gamemodeReferences, gamemodeDefinitions, contentIdentifier);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinition(gamemodeComponentReferences, gamemodeComponentDefinitions, contentIdentifier);
                case ContentType.Map:
                    return await LoadContentDefinition(mapReferences, mapDefinitions, contentIdentifier);
                case ContentType.Song:
                    return await LoadContentDefinition(songReferences, songDefinitions, contentIdentifier);
                case ContentType.Battle:
                    return await LoadContentDefinition(battleReferences, battleDefinitions, contentIdentifier);
                default:
                    return false;
            }
        }

        public List<IContentDefinition> GetContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Scenario:
                    return GetContentDefinitions(battleReferences, battleDefinitions);
                case ContentType.Fighter:
                    return GetContentDefinitions(fighterReferences, fighterDefinitions);
                case ContentType.Gamemode:
                    return GetContentDefinitions(gamemodeReferences, gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return GetContentDefinitions(gamemodeComponentReferences, gamemodeComponentDefinitions);
                case ContentType.Map:
                    return GetContentDefinitions(mapReferences, mapDefinitions);
                case ContentType.Song:
                    return GetContentDefinitions(songReferences, songDefinitions);
                case ContentType.Battle:
                    return GetContentDefinitions(battleReferences, battleDefinitions);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentIdentifier"></param>
        /// <returns></returns>
        public IContentDefinition GetContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Scenario:
                    return GetContentDefinition(battleDefinitions, contentIdentifier);
                case ContentType.Fighter:
                    return GetContentDefinition(fighterDefinitions, contentIdentifier);
                case ContentType.Gamemode:
                    return GetContentDefinition(gamemodeDefinitions, contentIdentifier);
                case ContentType.GamemodeComponent:
                    return GetContentDefinition(gamemodeComponentDefinitions, contentIdentifier);
                case ContentType.Map:
                    return GetContentDefinition(mapDefinitions, contentIdentifier);
                case ContentType.Song:
                    return GetContentDefinition(songDefinitions, contentIdentifier);
                case ContentType.Battle:
                    return GetContentDefinition(battleDefinitions, contentIdentifier);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        public void UnloadContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Scenario:
                    UnloadContentDefinitions(battleReferences, battleDefinitions);
                    break;
                case ContentType.Fighter:
                    UnloadContentDefinitions(fighterReferences, fighterDefinitions);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinitions(gamemodeReferences, gamemodeDefinitions);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinitions(gamemodeComponentReferences, gamemodeComponentDefinitions);
                    break;
                case ContentType.Map:
                    UnloadContentDefinitions(mapReferences, mapDefinitions);
                    break;
                case ContentType.Song:
                    UnloadContentDefinitions(songReferences, songDefinitions);
                    break;
                case ContentType.Battle:
                    UnloadContentDefinitions(battleReferences, battleDefinitions);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentIdentifier"></param>
        public void UnloadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Scenario:
                    UnloadContentDefinition(battleReferences, battleDefinitions, contentIdentifier);
                    break;
                case ContentType.Fighter:
                    UnloadContentDefinition(fighterReferences, fighterDefinitions, contentIdentifier);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinition(gamemodeReferences, gamemodeDefinitions, contentIdentifier);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinition(gamemodeComponentReferences, gamemodeComponentDefinitions, contentIdentifier);
                    break;
                case ContentType.Map:
                    UnloadContentDefinition(mapReferences, mapDefinitions, contentIdentifier);
                    break;
                case ContentType.Song:
                    UnloadContentDefinition(songReferences, songDefinitions, contentIdentifier);
                    break;
                case ContentType.Battle:
                    UnloadContentDefinition(battleReferences, battleDefinitions, contentIdentifier);
                    break;
            }
        }
        #endregion

        #region Shared
        protected async UniTask<bool> LoadContentDefinitions<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions) where T : IContentDefinition
        {
            // All of the content is already loaded.
            if (definitions.Count == references.Count)
            {
                return true;
            }
            try
            {
                foreach (string contentIdentifier in references.Keys)
                {
                    await LoadContentDefinition(references, definitions, contentIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            return false;
        }

        protected async UniTask<bool> LoadContentDefinition<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Content doesn't exist.
            if (references.ContainsKey(contentIdentifier) == false)
            {
                return false;
            }
            // Content already loaded.
            if (definitions.ContainsKey(contentIdentifier) == true)
            {
                return true;
            }
            OperationResult<T> result = await AddressablesManager.LoadAssetAsync<T>(references[contentIdentifier]);
            if (result.Succeeded)
            {
                result.Value.Identifier = contentIdentifier;
                definitions.Add(contentIdentifier, result);
                return true;
            }
            return false;
        }

        protected IContentDefinition GetContentDefinition<T>(Dictionary<string, OperationResult<T>> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Content does not exist, or was not loaded.
            if (definitions.ContainsKey(contentIdentifier) == false)
            {
                return null;
            }
            return definitions[contentIdentifier].Value;
        }

        protected List<IContentDefinition> GetContentDefinitions<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions) where T : IContentDefinition
        {
            List<IContentDefinition> contentList = new List<IContentDefinition>();
            foreach (var content in definitions.Values)
            {
                contentList.Add(content.Value);
            }
            return contentList;
        }

        protected void UnloadContentDefinitions<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions) where T : IContentDefinition
        {
            foreach (var v in definitions)
            {
                AddressablesManager.ReleaseAsset(references[v.Key]);
            }
            definitions.Clear();
        }

        protected void UnloadContentDefinition<T>(Dictionary<string, AssetReferenceT<T>> references,
            Dictionary<string, OperationResult<T>> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Fighter is not loaded.
            if (fighterDefinitions.ContainsKey(contentIdentifier) == false)
            {
                return;
            }
            AddressablesManager.ReleaseAsset(fighterReferences[contentIdentifier]);
            fighterDefinitions.Remove(contentIdentifier);
        }
        #endregion
    }
}