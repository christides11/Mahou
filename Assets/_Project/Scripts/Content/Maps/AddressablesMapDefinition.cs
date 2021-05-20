using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesMapDefinition", menuName = "Mahou/Content/Addressables/MapDefinition")]
    public class AddressablesMapDefinition : ScriptableObject, IMapDefinition
    {
        public string Identifier { get { return identifier; } }

        public string Name { get { return mapName; } }
        public List<string> SceneNames { get { return sceneNames; } }

        public string Description { get { return description; } }
        public bool Selectable { get { return selectable; } }

        [SerializeField] private string identifier;
        [SerializeField] private string mapName;
        [SerializeField] private List<string> sceneNames;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private bool selectable;

        [SerializeField] private AssetReference sceneReference;

        public async UniTask LoadScene()
        {
            await Addressables.LoadSceneAsync(sceneReference, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
        }

        public async UniTask UnloadScene()
        {

        }
    }
}