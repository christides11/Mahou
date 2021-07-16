using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BFall : FighterStateFall
    {
        public override void OnUpdate()
        {
            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("idle"),
                StateManager.CurrentStateFrame);

            FighterManager.PushboxManager.CreatePushboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("idle"),
                StateManager.CurrentStateFrame);

            base.OnUpdate();
        }
    }
}