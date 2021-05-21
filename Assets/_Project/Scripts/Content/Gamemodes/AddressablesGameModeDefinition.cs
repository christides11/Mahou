using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesGameModeDefinition", menuName = "Mahou/Content/Addressables/GameModeDefinition")]
    public class AddressablesGameModeDefinition : IGameModeDefinition
    {

        public override string Name { get { return gamemodeName; } }
        public override string Description { get { return description; } }
        public override bool BattleSelectionRequired { get { return battleSelectionRequired; } }
        public override bool MapSelectionRequired { get { return mapSelectionRequired;  } }

        [SerializeField] private string gamemodeName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReference gamemodeReference;
        [SerializeField] private bool battleSelectionRequired = true;
        [SerializeField] private bool mapSelectionRequired = false;

        [NonSerialized] private GameObject gamemode;

        public override async UniTask<bool> LoadGamemode()
        {
            if(gamemode != null)
            {
                return true;
            }

            try
            {
                var hh = await Addressables.LoadAssetAsync<GameObject>(gamemodeReference).Task;
                gamemode = hh;
                return true;
            }catch(Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public override GameModeBase GetGamemode()
        {
            if(gamemode == null)
            {
                return null;
            }
            return gamemode.GetComponent<GameModeBase>();
        }

        public override void UnloadGamemode()
        {
            Addressables.Release<GameObject>(gamemode);
        }
    }
}