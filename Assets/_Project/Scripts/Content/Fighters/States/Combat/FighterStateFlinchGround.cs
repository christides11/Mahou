using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateFlinchGround : FighterState
    {
        public override string GetName()
        {
            return $"Flinch (GROUND)";
        }

        public override void Initialize()
        {
            base.Initialize();
            //(Manager as FighterManager).entityAnimator.PlayAnimation((Manager as FighterManager).GetAnimationClip("hurt"));
        }

        public override void OnUpdate()
        {
            FighterManager e = FighterManager;
            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("idle"),
                StateManager.CurrentStateFrame);

            //e.PhysicsManager.ApplyMovementFriction(e.statManager.CurrentStats.hitstunFrictionGround.GetCurrentValue());
            PhysicsManager.ApplyMovementFriction(e.StatsManager.CurrentStats.hitstunFrictionXZ.GetCurrentValue());
            e.StateManager.IncrementFrame();

            //float f = (((float)e.StateManager.CurrentStateFrame / (float)e.CombatManager.HitStun) * 10.0f);
            //(Manager as FighterManager).entityAnimator.SetFrame((int)f);

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            FighterManager e = FighterManager;
            e.PhysicsManager.CheckIfGrounded();
            if (e.StateManager.CurrentStateFrame >= e.CombatManager.HitStun)
            {
                e.CombatManager.SetHitStun(0);
                // Hitstun finished.
                if (e.PhysicsManager.IsGrounded)
                {
                    e.StateManager.ChangeState((int)FighterStates.IDLE);
                }
                else
                {
                    e.StateManager.ChangeState((int)FighterStates.FALL);
                }
            }
            else if (e.PhysicsManager.IsGrounded == false)
            {
                e.StateManager.ChangeState((int)FighterStates.FLINCH_AIR, e.StateManager.CurrentStateFrame, false);
                return true;
            }
            return false;
        }
    }
}