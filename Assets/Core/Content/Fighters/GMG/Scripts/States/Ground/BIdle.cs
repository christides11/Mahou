using Mahou.Content.Fighters;
using UnityEngine;

namespace Mahou.Core
{
    public class BIdle : FighterStateIdle
    {

        public override void OnUpdate()
        {
            base.OnUpdate();
            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("idle"),
                StateManager.CurrentStateFrame);
        }
    }
}