using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BrawlerManager : FighterManager
    {
        public override FighterStats Stats { get { return stats; } protected set { } }

        public BrawlerStats stats;

        public override void SetupStates()
        {
            stateManager.AddState(new BIdle(), (ushort)BrawlerState.IDLE);
            stateManager.AddState(new BWalk(), (ushort)BrawlerState.WALK);
            stateManager.AddState(new BDash(), (ushort)BrawlerState.DASH);
            stateManager.AddState(new BRun(), (ushort)BrawlerState.RUN);
            stateManager.AddState(new BFall(), (ushort)BrawlerState.FALL);
            stateManager.AddState(new BJumpSquat(), (ushort)BrawlerState.JUMP_SQUAT);
            stateManager.AddState(new BJump(), (ushort)BrawlerState.JUMP);

            stateManager.ChangeState((ushort)BrawlerState.FALL);
        }
    }
}