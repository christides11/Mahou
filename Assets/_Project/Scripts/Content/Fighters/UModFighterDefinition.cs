using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    [CreateAssetMenu(fileName = "UModFighterDefinition", menuName = "Mahou/Content/UMod/FighterDefinition")]
    public class UModFighterDefinition : IFighterDefinition
    {
        public override string Name { get { return fighterName; } }
        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private bool selectable = true;

        public override GameObject GetFighter()
        {
            throw new NotImplementedException();
        }

        public override string GetFighterGUID()
        {
            throw new NotImplementedException();
        }

        public override MovesetDefinition[] GetMovesets()
        {
            throw new NotImplementedException();
        }

        public override UniTask<bool> LoadFighter()
        {
            throw new NotImplementedException();
        }

        public override void UnloadFighter()
        {
            throw new NotImplementedException();
        }
    }
}