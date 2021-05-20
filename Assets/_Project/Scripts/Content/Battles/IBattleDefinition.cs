using Cysharp.Threading.Tasks;
using Mahou.Content;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IBattleDefinition : ScriptableObject
    {
        public virtual string Identifier { get; }
        public virtual string Name { get; }
        public virtual string Description { get; }

        public virtual ModObjectReference MapReference { get; }


        public abstract UniTask<bool> LoadBattle();

        public abstract void UnloadBattle();
    }
}