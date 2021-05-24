using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IGameModeDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }
        public virtual bool BattleSelectionRequired { get; }
        public virtual bool MapSelectionRequired { get; }
        public virtual ModObjectReference[] GameModeComponentReferences { get; }

        public abstract UniTask<bool> LoadGamemode();
        public abstract GameModeBase GetGamemode();
        public abstract void UnloadGamemode();
    }
}