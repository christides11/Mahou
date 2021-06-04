using Cysharp.Threading.Tasks;
using Mahou.Content;
using Mahou.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class GameModeBandBattle : GameModeBase
    {
        private MusicMeterComponent musicMeterComponent;

        public override async UniTask<bool> LoadRequirements()
        {
            if(battleDefinition == null)
            {
                Debug.Log("BandBattle requires a battle.");
                return false;
            }
            return true;
        }

        public override void Initialize(IBattleDefinition battleDefinition = null)
        {
            base.Initialize(battleDefinition);

            GameObject gmGameObjectPrefab = ((IGameModeComponentDefinition)ContentManager.instance
                .GetContentDefinition(ContentType.GamemodeComponent, new ModObjectReference("core", "musicmeter")))
                .GetGamemodeComponent();

            GameObject go = GameObject.Instantiate(gmGameObjectPrefab, transform, false);
            musicMeterComponent = go.GetComponent<MusicMeterComponent>();
            musicMeterComponent.Init(this);
        }
    }
}