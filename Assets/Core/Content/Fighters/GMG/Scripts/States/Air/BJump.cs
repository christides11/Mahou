using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BJump : FighterStateJump
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if(StateManager.CurrentStateFrame == 5)
            {
                Simulation.SimulationAudioManager.Play((Manager as GMGManager).testAudioClip, Manager.transform.position, Simulation.AudioPlayMode.ROLLBACK);
            }
        }
    }
}
