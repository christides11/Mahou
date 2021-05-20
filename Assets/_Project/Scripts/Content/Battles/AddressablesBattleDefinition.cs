using Cysharp.Threading.Tasks;
using Mahou.Content;
using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesBattleDefinition", menuName = "Mahou/Content/Addressables/BattleDefinition")]
    public class AddressablesBattleDefinition : IBattleDefinition
    {
        public override string Identifier { get { return identifier; } }
        public override string Name { get { return battleName; } }
        public override string Description { get { return description; } }

        [SerializeField] private string identifier;
        [SerializeField] private string battleName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private ModObjectReference mapReference;

        public override UniTask<bool> LoadBattle()
        {
            throw new System.NotImplementedException();
        }

        public override void UnloadBattle()
        {

        }
    }
}