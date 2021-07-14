using Cysharp.Threading.Tasks;
using Mahou.Content;
using Mahou.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class GameModeScenario : GameModeBase
    {
        private MusicMeterComponent musicMeterComponent;

        public override async UniTask<bool> SetupGamemode(ModObjectReference[] componentReferences, List<ModObjectReference> content)
        {
            bool baseResult = await base.SetupGamemode(componentReferences, content);
            if (baseResult == false)
            {
                return false;
            }

            if (content.Count != 1)
            {
                return false;
            }

            bool battleLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Scenario, content[0]);
            if (battleLoadResult == false)
            {
                return false;
            }

            return true;
        }

        public override void Initialize()
        {
            base.Initialize();

            GameObject gmGameObjectPrefab = ((IGameModeComponentDefinition)ContentManager.instance
                .GetContentDefinition(ContentType.GamemodeComponent, new ModObjectReference("core", "musicmeter")))
                .GetGamemodeComponent();

            GameObject go = GameObject.Instantiate(gmGameObjectPrefab, transform, false);
            musicMeterComponent = go.GetComponent<MusicMeterComponent>();
            musicMeterComponent.Init(this);
        }
    }
}