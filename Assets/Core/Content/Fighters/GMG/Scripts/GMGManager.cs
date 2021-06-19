using Mahou.Content.Fighters;
using Mahou.Networking;
using Mahou.Simulation;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class GMGManager : FighterManager
    {
        public override FighterStatsManager StatsManager { get { return statManager; } }

        public GMGStatsManager statManager;
        public AudioClip testAudioClip;

        [Header("GMG GENERAL")]
        public bool isClone = false;
        public AssetIdentifier lightningGameobject;
        
        [Header("ABILITY: Encore")]
        public bool recordMode = false;
        public bool finishedRecording = false;
        public int maxRecordTime = 120;
        public int currentRecordIndex = 0;
        public HnSF.Input.InputRecordItem[] recordBuffer;

        public override void Initialize()
        {
            recordBuffer = new HnSF.Input.InputRecordItem[maxRecordTime];
            base.Initialize();
        }

        public override void Load()
        {
            NetworkClient.RegisterPrefab(lightningGameobject.gameObject, lightningGameobject.GetGUID());
            ISimStateSerializer.AddReaderWriter(GMGSimState.StaticGetGUID(), new GMGSimStateReaderWriter());
        }

        public override void Tick()
        {
            base.Tick();
            if (isClone)
            {
                return;
            }

            /*
            if(recordMode == true)
            {
                recordBuffer[currentRecordIndex] = InputManager.InputRecord[SimulationManagerBase.instance.CurrentTick % (int)InputManager.inputRecordSize];
                currentRecordIndex++;
                if(currentRecordIndex == maxRecordTime)
                {
                    recordMode = false;
                    finishedRecording = true;
                }
            } else if(finishedRecording == false)
            {
                if(inputManager.GetButton((int)PlayerInputType.ABILITY_ONE).firstPress == true)
                {
                    recordMode = true;
                }
            }*/
        }

        public override void SetupStates()
        {
            stateManager.AddState(new BIdle(), (ushort)FighterStates.IDLE);
            stateManager.AddState(new BWalk(), (ushort)FighterStates.WALK);
            stateManager.AddState(new BDash(), (ushort)FighterStates.DASH);
            stateManager.AddState(new BRun(), (ushort)FighterStates.RUN);
            stateManager.AddState(new BFall(), (ushort)FighterStates.FALL);
            stateManager.AddState(new BJumpSquat(), (ushort)FighterStates.JUMP_SQUAT);
            stateManager.AddState(new BJump(), (ushort)FighterStates.JUMP);
            stateManager.AddState(new BAirDash(), (ushort)FighterStates.AIR_DASH);
            stateManager.AddState(new BAirJump(), (ushort)FighterStates.AIR_JUMP);
            stateManager.AddState(new FighterStateAttack(), (ushort)FighterStates.ATTACK);

            stateManager.AddState(new FighterStateFlinchGround(), (ushort)FighterStates.FLINCH_GROUND);
            stateManager.AddState(new FighterStateFlinchAir(), (ushort)FighterStates.FLINCH_AIR);
            stateManager.AddState(new FighterStateTumble(), (ushort)FighterStates.TUMBLE);

            stateManager.ChangeState((ushort)FighterStates.FALL);
        }

        public override ISimState GetSimState()
        {
            GMGSimState gmgSimState = new GMGSimState();
            FillSimState(gmgSimState);
            gmgSimState.recordMode = recordMode;
            gmgSimState.finishedRecording = finishedRecording;
            gmgSimState.currentRecordingIndex = currentRecordIndex;
            gmgSimState.recordBuffer = recordBuffer;
            return gmgSimState;
        }

        public override void ApplySimState(ISimState state)
        {
            GMGSimState gmgSimState = state as GMGSimState;
            base.ApplySimState(state as PlayerSimState);
            recordMode = gmgSimState.recordMode;
            finishedRecording = gmgSimState.finishedRecording;
            currentRecordIndex = gmgSimState.currentRecordingIndex;
            recordBuffer = gmgSimState.recordBuffer;
        }
    }
}