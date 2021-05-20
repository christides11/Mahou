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

        bool GamemodeDefinitionsLoaded { get; }
        bool MapDefinitionsLoaded { get; }
        bool FighterDefinitionsLoaded { get; }
        bool BattleDefinitionsLoaded { get; }

        // GAMEMODES //
        UniTask<bool> LoadGamemodeDefinitions();
        List<IGameModeDefinition> GetGamemodeDefinitions();
        IGameModeDefinition GetGamemodeDefinition(string gamemodeIdentifier);
        void UnloadGamemodeDefinitions();

        // MAPS //
        UniTask LoadMapDefinitions();
        List<IMapDefinition> GetMapDefinitions();
        IMapDefinition GetMapDefinition(string mapIdentifier);
        void UnloadMapDefinitions();

        // FIGHTERS //
        UniTask LoadFighterDefinitions();
        List<IFighterDefinition> GetFighterDefinitions();
        IFighterDefinition GetFighterDefinition(string entityIdentifier);
        void UnloadFighterDefinitions();

        // BATTLES //
        UniTask LoadBattleDefinitions();
        List<IBattleDefinition> GetBattleDefinitions();
        IBattleDefinition GetBattleDefinition(string battleIdentifier);
        void UnloadBattleDefinitions();

    }
}