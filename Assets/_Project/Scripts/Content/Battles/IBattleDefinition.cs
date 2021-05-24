using Cysharp.Threading.Tasks;
using Mahou.Content;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IBattleDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }

        public virtual ModObjectReference MapReference { get; }


        public abstract UniTask<bool> LoadBattle();
        public abstract Battle GetBattle();
        public abstract void UnloadBattle();
    }
}