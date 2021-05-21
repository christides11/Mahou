using Cysharp.Threading.Tasks;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mahou.Content
{
    public interface IModDefinition
    {
        string Description { get; }

        // FIGHTERS //
        UniTask<bool> LoadFighterDefinitions();
        UniTask<bool> LoadFighterDefinition(string fighterIdentifier);
        List<IFighterDefinition> GetFighterDefinitions();
        IFighterDefinition GetFighterDefinition(string entityIdentifier);
        void UnloadFighterDefinition(string fighterIdentifier);
        void UnloadFighterDefinitions();

        // GAMEMODES //
        UniTask<bool> LoadGamemodeDefinitions();
        UniTask<bool> LoadGamemodeDefinition(string gamemodeIdentifier);
        List<IGameModeDefinition> GetGamemodeDefinitions();
        IGameModeDefinition GetGamemodeDefinition(string gamemodeIdentifier);
        void UnloadGamemodeDefinition(string gamemodeIdentifier);
        void UnloadGamemodeDefinitions();

        // MAPS //
        UniTask<bool> LoadMapDefinitions();
        UniTask<bool> LoadMapDefinition(string mapIdentifier);
        List<IMapDefinition> GetMapDefinitions();
        IMapDefinition GetMapDefinition(string mapIdentifier);
        void UnloadMapDefinition(string mapIdentifier);
        void UnloadMapDefinitions();

        // BATTLES //
        UniTask<bool> LoadBattleDefinitions();
        UniTask<bool> LoadBattleDefinition(string battleIdentifier);
        List<IBattleDefinition> GetBattleDefinitions();
        IBattleDefinition GetBattleDefinition(string battleIdentifier);
        void UnloadBattleDefinition(string battleIdentifier);
        void UnloadBattleDefinitions();

    }
}