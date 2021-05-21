using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IGameModeDefinition : ScriptableObject
    {
        public virtual string Identifier { get; set; } = "";
        public virtual string Name { get; }
        public virtual string Description { get; }
        public virtual bool BattleSelectionRequired { get; }
        public virtual bool MapSelectionRequired { get; }

        public abstract UniTask<bool> LoadGamemode();
        public abstract GameModeBase GetGamemode();
        public abstract void UnloadGamemode();
    }
}