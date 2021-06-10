using HnSF.Combat;
using HnSF.Fighters;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.Events
{
    public class PlaySound : HnSF.Combat.AttackEvent
    {
        public AudioClip sound;

        public override string GetName()
        {
            return "Play Sound";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame, FighterBase manager, AttackEventVariables variables)
        {
            Simulation.SimulationAudioManager.Play(sound, manager.visual.transform.position, Simulation.AudioPlayMode.ROLLBACK);
            return AttackEventReturnType.NONE;
        }
    }
}