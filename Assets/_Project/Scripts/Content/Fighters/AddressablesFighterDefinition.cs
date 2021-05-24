using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Mahou.Content.Fighters
{
    [CreateAssetMenu(fileName = "AddressablesFighterDefinition", menuName = "Mahou/Content/Addressables/FighterDefinition")]
    public class AddressablesFighterDefinition : IFighterDefinition
    {
        public override string Name { get { return fighterName; } }
        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReferenceT<GameObject> fighterReference;
        [SerializeField] private AssetReference fighterTestReference;
        [SerializeField] private AssetReferenceT<MovesetDefinition>[] movesetReferences;
        [SerializeField] private bool selectable = true;

        [NonSerialized] private MovesetDefinition[] movesets = null;
        [NonSerialized] private GameObject fighter = null;

        public override async UniTask<bool> LoadFighter()
        {
            if (fighter != null)
            {
                return true;
            }

            // Load fighter.
            try
            {
                await fighterTestReference.LoadAssetAsync<GameObject>();
                fighter = (GameObject)fighterTestReference.Asset;
                //OperationResult<GameObject> fighterLoadResult = await AddressablesManager.LoadAssetAsync(fighterReference);
                //fighter = fighterLoadResult.Value;
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
                    var movesetLoadResult = await AddressablesManager.LoadAssetAsync(movesetReferences[i]);
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
            return fighter;
        }

        public override string GetFighterGUID()
        {
            return fighterTestReference.AssetGUID;
        }

        public override MovesetDefinition[] GetMovesets()
        {
            return movesets;
        }

        public override void UnloadFighter()
        {
            fighter = null;
            movesets = null;
            for(int i = 0; i < movesetReferences.Length; i++)
            {
                AddressablesManager.ReleaseAsset(movesetReferences[i]);
            }
            AddressablesManager.ReleaseAsset(fighterReference);
        }
    }
}