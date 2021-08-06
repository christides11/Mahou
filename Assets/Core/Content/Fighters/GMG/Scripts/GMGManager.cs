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
        public AssetIdentifier[] extras;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Load()
        {
            for (int i = 0; i < extras.Length; i++)
            {
                NetworkClient.RegisterPrefab(extras[i].gameObject, extras[i].GetGUID());
            }
            ISimStateSerializer.AddReaderWriter(GMGSimState.StaticGetGUID(), new GMGSimStateReaderWriter());
        }

        public override void Tick()
        {
            base.Tick();
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

            stateManager.AddState(new FighterStateBlockHigh(), (ushort)FighterStates.BLOCK_HIGH);
            stateManager.AddState(new FighterStateBlockLow(), (ushort)FighterStates.BLOCK_LOW);
            stateManager.AddState(new FighterStateBlockAir(), (ushort)FighterStates.BLOCK_AIR);

            stateManager.AddState(new FighterStateFlinchGround(), (ushort)FighterStates.FLINCH_GROUND);
            stateManager.AddState(new FighterStateFlinchAir(), (ushort)FighterStates.FLINCH_AIR);
            stateManager.AddState(new FighterStateTumble(), (ushort)FighterStates.TUMBLE);

            stateManager.ChangeState((ushort)FighterStates.FALL);
        }

        public override ISimState GetSimState()
        {
            GMGSimState gmgSimState = new GMGSimState();
            FillSimState(gmgSimState);
            return gmgSimState;
        }

        public override void ApplySimState(ISimState state)
        {
            GMGSimState gmgSimState = state as GMGSimState;
            base.ApplySimState(state as PlayerSimState);
        }
    }
}