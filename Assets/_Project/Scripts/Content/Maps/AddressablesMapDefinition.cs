using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesMapDefinition", menuName = "Mahou/Content/Addressables/MapDefinition")]
    public class AddressablesMapDefinition : IMapDefinition
    {
        public override string Name { get { return mapName; } }
        public override List<string> SceneNames { get { return sceneNames; } }

        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string mapName;
        [SerializeField] private List<string> sceneNames;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private bool selectable;

        [SerializeField] private AssetReference sceneReference;

        public override async UniTask LoadMap()
        {
            await Addressables.LoadSceneAsync(sceneReference, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
        }

        public override async UniTask UnloadMap()
        {

        }
    }
}