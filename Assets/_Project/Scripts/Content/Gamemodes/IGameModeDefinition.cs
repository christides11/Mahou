using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public interface IGameModeDefinition
    {
        public string Identifier { get; }
        public string Name { get; }
        public string Description { get; }
        public bool BattleSelectionRequired { get; }
        public bool MapSelectionRequired { get; }

        public UniTask<bool> LoadGamemode();
        public GameModeBase GetGamemode();
        public void UnloadGamemode();
    }
}