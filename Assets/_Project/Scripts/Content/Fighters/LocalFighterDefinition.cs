using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Mahou.Content.Fighters
{
    [CreateAssetMenu(fileName = "LocalFighterDefinition", menuName = "Content/Local/FighterDefinition")]
    public class LocalFighterDefinition : ScriptableObject, IFighterDefinition
    {

        public string Identifier { get { return identifier; } }

        public string Name { get { return fighterName; } }

        public string Description { get { return description; } }

        [SerializeField] private string identifier;
        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReference fighterReference;

        [NonSerialized] private GameObject fighter;

        public async UniTask<bool> LoadFighter()
        {
            if (fighter != null)
            {
                return true;
            }

            try
            {
                var hh = await Addressables.LoadAssetAsync<GameObject>(fighterReference).Task;
                fighter = hh;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public GameObject GetFighter()
        {
            if (fighter == null)
            {
                return null;
            }
            return fighter;
        }

        public void UnloadFighter()
        {
            Addressables.Release<GameObject>(fighter);
        }
    }
}