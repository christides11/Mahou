using Cysharp.Threading.Tasks;
using Mahou.Content;
using Mahou.Managers;
using Mahou.Networking;
using Mahou.Simulation;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class GameModeWaveBattle : GameModeBase
    {
        private IBattleDefinition waveBattle;
        private GameModeComponent musicMeter;

        public int initTimer = 180;

        public override async UniTask<bool> SetupGamemode(ModObjectReference[] componentReferences, List<ModObjectReference> content)
        {
            bool baseResult = await base.SetupGamemode(componentReferences, content);
            if (baseResult == false)
            {
                return false;
            }

            if (content.Count != 2)
            {
                return false;
            }

            bool battleLoadResult = await ContentManager.instance.LoadContentDefinition(ContentType.Battle, content[0]);
            if(battleLoadResult == false)
            {
                return false;
            }
            waveBattle = (IBattleDefinition)ContentManager.instance.GetContentDefinition(ContentType.Battle, content[0]);

            bool b = await waveBattle.LoadBattle();
            if(b == false)
            {
                return false;
            }

            bool mapLoadResult = await GameManager.current.LoadMap(content[1]);
            if (mapLoadResult == false)
            {
                return false;
            }

            return true;
        }

        WaveEnemySpawner enemySpawner;

        public override void Initialize()
        {
            base.Initialize();

            GameObject musicMeterPrefab = ((IGameModeComponentDefinition)ContentManager.instance
                .GetContentDefinition(ContentType.GamemodeComponent, new ModObjectReference("core", "musicmeter")))
                .GetGamemodeComponent();

            musicMeter = GameObject.Instantiate(musicMeterPrefab, transform, false).GetComponent<GameModeComponent>();
        }

        public override void OnStartMatch()
        {
            base.OnStartMatch();
            enemySpawner = GameObject.FindObjectOfType<WaveEnemySpawner>();
            initTimer = 180;
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
        }

        public override void Tick()
        {
            base.Tick();
            switch (gameModeState)
            {
                case GameModeState.PRE_MATCH:
                    initTimer -= 1;
                    if (initTimer == 0)
                    {
                        SetGameModeState(GameModeState.MATCH_IN_PROGRESS);
                        InitialSpawn();
                    }
                    break;
                case GameModeState.MATCH_IN_PROGRESS:
                    
                    break;
            }
        }

        private void InitialSpawn()
        {
            int spawnIndex = 0;
            Battle b = waveBattle.GetBattle();
            for (int i = 0; i < b.waves[0].enemyGroups[0].enemies.Length; i++)
            {
                IFighterDefinition g = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, b.waves[0].enemyGroups[0].enemies[i].reference);
                GameObject spawnPoint = enemySpawner.spawnPoints[spawnIndex % enemySpawner.spawnPoints.Length];
                SimulationCreationManager.Create(g.GetFighter().GetComponent<AssetIdentifier>(), spawnPoint.transform.position, spawnPoint.transform.rotation);
                spawnIndex++;
            }
        }

        public override void LateTick()
        {
            base.LateTick();
        }

        public override GameModeBaseSimState GetSimState()
        {
            GameModeWaveBattleSimState ss = new GameModeWaveBattleSimState()
            {
                gameModeState = gameModeState,
                initTimer = initTimer
            };
            return ss;
        }

        public override void ApplySimState(GameModeBaseSimState simState)
        {
            GameModeWaveBattleSimState ss = simState as GameModeWaveBattleSimState;
            gameModeState = ss.gameModeState;
            initTimer = ss.initTimer;
        }
    }
}