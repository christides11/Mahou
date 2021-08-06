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
            //Simulation.SimulationAudioManager.Play((Manager as GMGManager).testAudioClip, Manager.transform.position, Simulation.AudioPlayMode.ROLLBACK);
        }

        public override void OnUpdate()
        {
            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("idle"),
                StateManager.CurrentStateFrame);

            FighterManager.PushboxManager.CreatePushboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetPushbox("idle"),
                StateManager.CurrentStateFrame);

            base.OnUpdate();
        }
    }
}
