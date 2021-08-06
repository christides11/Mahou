using Cysharp.Threading.Tasks;
using Mahou.Content;
using Mahou.Managers;
using Mahou.Networking;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class GameModeTraining : GameModeBase
    {
        public ModObjectReference testReference;

        public NetworkIdentity trainingDummy;

        public override async UniTask<bool> SetupGamemode(ModObjectReference[] componentReferences, List<ModObjectReference> content)
        {
            bool baseResult = await base.SetupGamemode(componentReferences, content);
            if (baseResult == false)
            {
                return false;
            }

            if(content.Count != 1)
            {
                return false;
            }

            bool mapLoadResult = await GameManager.current.LoadMap(content[0]);
            if(mapLoadResult == false)
            {
                return false;
            }

            return true;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnStartMatch()
        {
            base.OnStartMatch();
            if (NetworkServer.active)
            {
                SpawnPointManager spm = GameObject.FindObjectOfType<SpawnPointManager>();
                int sIndex = 0;
                foreach (var c in ClientManager.clientManagers)
                {
                    c.Value.SpawnPlayerFighter(c.Value.fighters[0], spm.spawnPoints[sIndex].transform.position, spm.spawnPoints[sIndex].transform.rotation);
                    sIndex++;
                }
            }

            SetGameModeState(GameModeState.MATCH_IN_PROGRESS);
        }

        public override void GMUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8))
            {
                SpawnTrainingDummy();
            }
        }

        public async void SpawnTrainingDummy()
        {
            // Server only.
            if(NetworkServer.active == false)
            {
                return;
            }

            bool requestResult = await NetworkFighterSpawnManager.ServerRequestFighterLoad(testReference, 5.0f);
            if (requestResult == false)
            {
                return;
            }

            IFighterDefinition fighterDefinition = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, testReference);
            if (fighterDefinition == null)
            {
                return;
            }
            var fighterGO = fighterDefinition.GetFighter();
            if (fighterGO == null)
            {
                return;
            }
            GameObject fighter = GameObject.Instantiate(fighterGO, new Vector3(0, 1, 0), Quaternion.identity);
            NetworkServer.Spawn(fighter);
            Mahou.Simulation.SimulationManagerBase.instance.RegisterSimulationObject(fighter.GetComponent<NetworkIdentity>());
            Debug.Log("Spawning training dummy.");
            trainingDummy = fighter.GetComponent<NetworkIdentity>();
        }
    }
}