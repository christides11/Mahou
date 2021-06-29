using Mahou.Content;
using Mahou.Managers;
using Mirror;
using UnityEngine;

namespace Mahou.Core
{
    public class GameModeTraining : GameModeBase
    {
        public ModObjectReference testReference;

        public override void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8))
            {
                SpawnFighter();
            }
        }

        public async void SpawnFighter()
        {
            // Server only.
            if(NetworkServer.active == false)
            {
                return;
            }

            bool requestResult = await NetworkFighterSpawnManager.ServerRequestFighterLoad(testReference, 5.0f);

            if (requestResult)
            {
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
                Debug.Log("Spawning");
            }
        }
    }
}