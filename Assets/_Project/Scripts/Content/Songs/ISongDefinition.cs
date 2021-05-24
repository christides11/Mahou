using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class ISongDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }

        public abstract UniTask<bool> LoadSong();
        public abstract Song GetSong();
        public abstract void UnloadSong();
    }
}