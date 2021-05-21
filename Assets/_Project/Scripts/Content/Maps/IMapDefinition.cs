using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IMapDefinition : ScriptableObject
    {
        public virtual string Identifier { get; set; } = "";
        public virtual string Name { get; }
        public virtual List<string> SceneNames { get; }
        public virtual string Description { get; }
        public virtual bool Selectable { get; }

        public abstract UniTask LoadMap();
        public abstract UniTask UnloadMap();
    }
}