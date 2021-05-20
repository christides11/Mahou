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
    public class AddressablesGameModeDefinition : ScriptableObject, IGameModeDefinition
    {
        public string Identifier { get { return identifier; } }

        public string Name { get { return gamemodeName; } }

        public string Description { get { return description; } }
        public bool BattleSelectionRequired { get { return battleSelectionRequired; } }
        public bool MapSelectionRequired { get { return mapSelectionRequired;  } }


        [SerializeField] private string identifier;
        [SerializeField] private string gamemodeName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReference gamemodeReference;
        [SerializeField] private bool battleSelectionRequired = true;
        [SerializeField] private bool mapSelectionRequired = false;

        [NonSerialized] private GameObject gamemode;

        public async UniTask<bool> LoadGamemode()
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
                return false;
            }
        }

        public GameModeBase GetGamemode()
        {
            if(gamemode == null)
            {
                return null;
            }
            return gamemode.GetComponent<GameModeBase>();
        }

        public void UnloadGamemode()
        {
            Addressables.Release<GameObject>(gamemode);
        }
    }
}