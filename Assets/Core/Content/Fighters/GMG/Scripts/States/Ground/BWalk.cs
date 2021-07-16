using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BWalk : FighterStateWalk
    {

        public override void OnUpdate()
        {
            FighterManager.PushboxManager.CreatePushboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("idle"),
                StateManager.CurrentStateFrame);

            base.OnUpdate();
        }

    }
}