using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Mahou.Content.Fighters
{
    [CreateAssetMenu(fileName = "AddressablesFighterDefinition", menuName = "Mahou/Content/Addressables/FighterDefinition")]
    public class AddressablesFighterDefinition : IFighterDefinition
    {
        public override string Identifier { get { return identifier; } }

        public override string Name { get { return fighterName; } }

        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string identifier;
        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReference fighterReference;
        [SerializeField] private AssetReference[] movesetReferences;
        [SerializeField] private bool selectable = true;

        [NonSerialized] private MovesetDefinition[] movesets;
        [NonSerialized] private GameObject fighter;

        public override async UniTask<bool> LoadFighter()
        {
            if (fighter != null)
            {
                return true;
            }

            // Load fighter.
            try
            {
                var fighterLoadResult = await Addressables.LoadAssetAsync<GameObject>(fighterReference).Task;
                fighter = fighterLoadResult;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            // Load movesets.
            try
            {
                movesets = new MovesetDefinition[movesetReferences.Length];
                for(int i = 0; i < movesetReferences.Length; i++)
                {
                    var movesetLoadResult = await Addressables.LoadAssetAsync<MovesetDefinition>(movesetReferences[i]).Task;
                    movesets[i] = movesetLoadResult;
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            return true;
        }

        public override GameObject GetFighter()
        {
            if (fighter == null)
            {
                return null;
            }
            return fighter;
        }

        public override MovesetDefinition[] GetMovesets()
        {
            return movesets;
        }

        public override void UnloadFighter()
        {
            Addressables.Release<GameObject>(fighter);
        }
    }
}