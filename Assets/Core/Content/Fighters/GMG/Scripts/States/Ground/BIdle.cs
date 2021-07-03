using Mahou.Content.Fighters;
using UnityEngine;

namespace Mahou.Core
{
    public class BIdle : FighterStateIdle
    {
        public override void Initialize()
        {
            base.Initialize();
            (Manager as FighterManager).fighterAnimator
                .PlayAnimation((Manager as FighterManager).GetAnimationClip("idle", Manager.CombatManager.CurrentMovesetIdentifier));
        }

        public override void OnUpdate()
        {
            PhysicsManager.ApplyMovementFriction();
            if ((Manager as FighterManager).LockedOn)
            {
                (Manager as FighterManager).RotateVisual((Manager as FighterManager).LockonForward, 10);
            }

            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("idle"),
                StateManager.CurrentStateFrame);
            if (Simulation.SimulationManagerBase.IsRollbackFrame == false
                || Simulation.SimulationManagerBase.instance.CurrentRollbackTick == Simulation.SimulationManagerBase.instance.CurrentRealTick-1)
            {
                (Manager as FighterManager).fighterAnimator.SetFrame((int)Manager.StateManager.CurrentStateFrame);
            }

            if (CheckInterrupt() == false)
            {
                Manager.StateManager.IncrementFrame();
            }
        }
    }
}