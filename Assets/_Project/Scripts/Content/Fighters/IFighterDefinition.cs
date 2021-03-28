using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mahou.Content
{
    public interface IFighterDefinition
    {
        public string Identifier { get; }
        public string Name { get; }
        public string Description { get; }

        public UniTask<bool> LoadFighter();
        public GameObject GetFighter();
        public void UnloadFighter();
    }
}