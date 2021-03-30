using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BrawlerManager : FighterManager
    {

        public override void SetupStates()
        {
            stateManager.AddState(new BIdle(), (ushort)BrawlerState.IDLE);
            stateManager.AddState(new BWalk(), (ushort)BrawlerState.WALK);

            //stateManager.ChangeState((ushort)BrawlerState.IDLE);
        }
    }
}