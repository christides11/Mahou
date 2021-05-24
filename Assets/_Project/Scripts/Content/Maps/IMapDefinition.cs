using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IMapDefinition : IContentDefinition
    {
        public override string Name { get; }
        public virtual List<string> SceneNames { get; }
        public override string Description { get; }
        public virtual bool Selectable { get; }

        public abstract UniTask LoadMap();
        public abstract UniTask UnloadMap();
    }
}