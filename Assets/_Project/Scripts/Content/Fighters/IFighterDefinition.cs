using Cysharp.Threading.Tasks;
using Mahou.Content.Fighters;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IFighterDefinition : ScriptableObject
    {
        public virtual string Identifier { get; }
        public virtual string Name { get; }
        public virtual string Description { get; }
        public virtual bool Selectable { get; }

        /// <summary>
        /// Loads everything related to the fighter. Call this before any other method.
        /// </summary>
        /// <returns>True if the load was successful.</returns>
        public abstract UniTask<bool> LoadFighter();

        public abstract GameObject GetFighter();

        public abstract MovesetDefinition[] GetMovesets();
        /// <summary>
        /// Unloads everything related to the fighter. Call this when the fighter is no longer needed.
        /// </summary>
        public abstract void UnloadFighter();
    }
}