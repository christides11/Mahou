using Cysharp.Threading.Tasks;
using Mahou.Content;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesBattleDefinition", menuName = "Mahou/Content/Addressables/BattleDefinition")]
    public class AddressablesBattleDefinition : IBattleDefinition
    {
        public override string Name { get { return battleName; } }
        public override string Description { get { return description; } }

        [SerializeField] private string battleName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReferenceT<Battle> battleReference;

        [NonSerialized] private Battle battle;

        public override async UniTask<bool> LoadBattle()
        {
            if (battle != null)
            {
                return true;
            }

            // Load fighter.
            try
            {
                var loadResult = await AddressablesManager.LoadAssetAsync(battleReference);
                battle = loadResult.Value;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public override Battle GetBattle()
        {
            return battle;
        }

        public override void UnloadBattle()
        {
            battle = null;
            AddressablesManager.ReleaseAsset(battleReference);
        }
    }
}