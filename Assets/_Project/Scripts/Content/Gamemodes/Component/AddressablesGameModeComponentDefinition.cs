using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesGameModeComponentDefinition", menuName = "Mahou/Content/Addressables/GameModeComponentDefinition")]
    public class AddressablesGameModeComponentDefinition : IGameModeComponentDefinition
    {

        public override string Name { get { return componentName; } }
        public override string Description { get { return description; } }

        [SerializeField] private string componentName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReferenceT<GameObject> componentReference;

        [NonSerialized] private GameObject gamemodeComponent;

        public override async UniTask<bool> LoadGamemodeComponent()
        {
            if (gamemodeComponent != null)
            {
                return true;
            }

            try
            {
                var hh = await AddressablesManager.LoadAssetAsync(componentReference);
                gamemodeComponent = hh.Value;
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public override GameObject GetGamemodeComponent()
        {
            return gamemodeComponent;
        }

        public override void UnloadGamemodeComponent()
        {
            AddressablesManager.ReleaseAsset(componentReference);
        }
    }
}
