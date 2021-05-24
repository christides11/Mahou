using Cysharp.Threading.Tasks;
using Mahou.Debugging;
using Mahou.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UMod;
using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "UModModDefinition", menuName = "Mahou/Content/UMod/ModDefinition")]
    public class UModModDefinition : ScriptableObject, IModDefinition
    {
        [System.Serializable]
        public class AssetReferenceRelation
        {
            public string identifier;
            public string assetPath;
        }

        public string Description { get { return description; } }

        [TextArea] [SerializeField] private string description;
        [SerializeField] private ModObjectSharedReference modNamespace;

        [SerializeField] private List<AssetReferenceRelation> fighters = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> gamemodes = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> gamemodeComponents = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> maps = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> battles = new List<AssetReferenceRelation>();
        [SerializeField] private List<AssetReferenceRelation> songs = new List<AssetReferenceRelation>();

        [NonSerialized] private Dictionary<string, string> fighterPaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IFighterDefinition> fighterDefinitions = new Dictionary<string, IFighterDefinition>();

        [NonSerialized] private Dictionary<string, string> gamemodePaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IGameModeDefinition> gamemodeDefinitions = new Dictionary<string, IGameModeDefinition>();

        [NonSerialized] private Dictionary<string, string> gamemodeComponentPaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IGameModeComponentDefinition> gamemodeComponentDefinitions = new Dictionary<string, IGameModeComponentDefinition>();

        [NonSerialized] private Dictionary<string, string> mapPaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IMapDefinition> mapDefinitions = new Dictionary<string, IMapDefinition>();

        [NonSerialized] private Dictionary<string, string> battlePaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, IBattleDefinition> battleDefinitions = new Dictionary<string, IBattleDefinition>();

        [NonSerialized] private Dictionary<string, string> songPaths = new Dictionary<string, string>();
        [NonSerialized] private Dictionary<string, ISongDefinition> songDefinitions = new Dictionary<string, ISongDefinition>();

        public void OnEnable()
        {
            fighterPaths.Clear();
            gamemodePaths.Clear();
            gamemodeComponents.Clear();
            mapPaths.Clear();
            battlePaths.Clear();
            songPaths.Clear();
            foreach(AssetReferenceRelation r in fighters)
            {
                fighterPaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in gamemodes)
            {
                gamemodePaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in gamemodeComponents)
            {
                gamemodeComponentPaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in maps)
            {
                mapPaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in battles)
            {
                battlePaths.Add(r.identifier, r.assetPath);
            }
            foreach (AssetReferenceRelation r in songs)
            {
                songPaths.Add(r.identifier, r.assetPath);
            }
        }

        #region Content
        public async UniTask<bool> LoadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            ModHost modHost = ModManager.instance.ModLoader.loadedMods[modNamespace.reference.modIdentifier].host;
            switch (contentType)
            {
                case ContentType.Fighter:
                    return await LoadContentDefinition(modHost, fighterPaths, fighterDefinitions, contentIdentifier);
                case ContentType.Battle:
                    return await LoadContentDefinition(modHost, battlePaths, battleDefinitions, contentIdentifier);
                case ContentType.Gamemode:
                    return await LoadContentDefinition(modHost, gamemodePaths, gamemodeDefinitions, contentIdentifier);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinition(modHost, gamemodeComponentPaths, gamemodeComponentDefinitions, contentIdentifier);
                case ContentType.Map:
                    return await LoadContentDefinition(modHost, mapPaths, mapDefinitions, contentIdentifier);
                case ContentType.Song:
                    return await LoadContentDefinition(modHost, songPaths, songDefinitions, contentIdentifier);
                default:
                    return false;
            }
        }

        public async UniTask<bool> LoadContentDefinitions(ContentType contentType)
        {
            ModHost modHost = ModManager.instance.ModLoader.loadedMods[modNamespace.reference.modIdentifier].host;
            switch (contentType)
            {
                case ContentType.Fighter:
                    return await LoadContentDefinitions(modHost, fighterPaths, fighterDefinitions);
                case ContentType.Battle:
                    return await LoadContentDefinitions(modHost, battlePaths, battleDefinitions);
                case ContentType.Gamemode:
                    return await LoadContentDefinitions(modHost, gamemodePaths, gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return await LoadContentDefinitions(modHost, gamemodeComponentPaths, gamemodeComponentDefinitions);
                case ContentType.Map:
                    return await LoadContentDefinitions(modHost, mapPaths, mapDefinitions);
                case ContentType.Song:
                    return await LoadContentDefinitions(modHost, songPaths, songDefinitions);
                default:
                    return false;
            }
        }

        public IContentDefinition GetContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Battle:
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
                default:
                    return null;
            }
        }

        public List<IContentDefinition> GetContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Battle:
                    return GetContentDefinitions(battleDefinitions);
                case ContentType.Fighter:
                    return GetContentDefinitions(fighterDefinitions);
                case ContentType.Gamemode:
                    return GetContentDefinitions(gamemodeDefinitions);
                case ContentType.GamemodeComponent:
                    return GetContentDefinitions(gamemodeComponentDefinitions);
                case ContentType.Map:
                    return GetContentDefinitions(mapDefinitions);
                case ContentType.Song:
                    return GetContentDefinitions(songDefinitions);
                default:
                    return null;
            }
        }

        public void UnloadContentDefinition(ContentType contentType, string contentIdentifier)
        {
            switch (contentType)
            {
                case ContentType.Battle:
                    UnloadContentDefinition(battleDefinitions, contentIdentifier);
                    break;
                case ContentType.Fighter:
                    UnloadContentDefinition(fighterDefinitions, contentIdentifier);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinition(gamemodeDefinitions, contentIdentifier);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinition(gamemodeComponentDefinitions, contentIdentifier);
                    break;
                case ContentType.Map:
                    UnloadContentDefinition(mapDefinitions, contentIdentifier);
                    break;
                case ContentType.Song:
                    UnloadContentDefinition(songDefinitions, contentIdentifier);
                    break;
            }
        }

        public void UnloadContentDefinitions(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Battle:
                    UnloadContentDefinitions(battleDefinitions);
                    break;
                case ContentType.Fighter:
                    UnloadContentDefinitions(fighterDefinitions);
                    break;
                case ContentType.Gamemode:
                    UnloadContentDefinitions(gamemodeDefinitions);
                    break;
                case ContentType.GamemodeComponent:
                    UnloadContentDefinitions(gamemodeComponentDefinitions);
                    break;
                case ContentType.Map:
                    UnloadContentDefinitions(mapDefinitions);
                    break;
                case ContentType.Song:
                    UnloadContentDefinitions(songDefinitions);
                    break;
            }
        }
#endregion

        #region Shared
        protected async UniTask<bool> LoadContentDefinitions<T>(ModHost modHost, Dictionary<string, string> paths,
            Dictionary<string, T> definitions) where T : IContentDefinition
        {
            // All of the content is already loaded.
            if (paths.Count == definitions.Count)
            {
                return true;
            }
            try
            {
                foreach (string contentIdentifier in paths.Keys)
                {
                    await LoadContentDefinition(modHost, paths, definitions, contentIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            return false;
        }

        protected async UniTask<bool> LoadContentDefinition<T>(ModHost modHost, Dictionary<string, string> paths,
            Dictionary<string, T> definitions, string contentIdentifier) 
            where T : IContentDefinition
        {
            // Asset doesn't exist.
            if (paths.ContainsKey(contentIdentifier) == false)
            {
                ConsoleWindow.current.WriteLine($"{contentIdentifier} does not exist for {modHost.name}.");
                return false;
            }
            // Content already loaded.
            if(definitions.ContainsKey(contentIdentifier) == true)
            {
                return true;
            }
            // Invalid path.
            if (modHost.Assets.Exists(paths[contentIdentifier]) == false)
            {
                ConsoleWindow.current.WriteLine($"Content does not exist at path {paths[contentIdentifier]} for {modHost.name}.");
                return false;
            }
            ModAsyncOperation request = modHost.Assets.LoadAsync(paths[contentIdentifier]);
            await request;
            if (request.IsSuccessful)
            {
                (request.Result as T).Identifier = contentIdentifier;
                definitions.Add(contentIdentifier, request.Result as T);
                return true;
            }
            return false;
        }

        protected IContentDefinition GetContentDefinition<T>(Dictionary<string, T> definitions, string contentIdentifier)
            where T : IContentDefinition
        {
            // Content does not exist, or was not loaded.
            if (definitions.ContainsKey(contentIdentifier) == false)
            {
                return null;
            }
            return definitions[contentIdentifier];
        }

        protected List<IContentDefinition> GetContentDefinitions<T>(Dictionary<string, T> definitions) where T : IContentDefinition
        {
            List<IContentDefinition> contentList = new List<IContentDefinition>();
            foreach (var content in definitions.Values)
            {
                contentList.Add(content);
            }
            return contentList;
        }

        protected void UnloadContentDefinitions<T>(Dictionary<string, T> definitions) where T : IContentDefinition
        {
            definitions.Clear();
        }

        protected void UnloadContentDefinition<T>(Dictionary<string, T> definitions, string contentIdentifier) where T : IContentDefinition
        {
            // Fighter is not loaded.
            if (fighterDefinitions.ContainsKey(contentIdentifier) == false)
            {
                return;
            }
            fighterDefinitions.Remove(contentIdentifier);
        }
        #endregion
    }
}