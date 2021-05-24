using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IGameModeComponentDefinition : IContentDefinition
    {
        public abstract UniTask<bool> LoadGamemodeComponent();
        public abstract GameObject GetGamemodeComponent();
        public abstract void UnloadGamemodeComponent();
    }
}