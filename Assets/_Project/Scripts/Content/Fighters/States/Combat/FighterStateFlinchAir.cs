using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateFlinchAir : FighterState
    {
        public override string GetName()
        {
            return $"Flinch (Air)";
        }

        public override void Initialize()
        {
            base.Initialize();
            heldFrames = 0;
            //(Manager as FighterManager).entityAnimator.PlayAnimation((Manager as FighterManager).GetAnimationClip("hurt"));
        }

        int heldFrames = 0;
        public override void OnUpdate()
        {
            FighterManager e = FighterManager;
            FighterCombatManager cm = (FighterCombatManager)e.CombatManager;
            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("flinch-air"),
                StateManager.CurrentStateFrame);

            if(PhysicsManager.forceGravity.y == 0 && heldFrames < 15)
            {
                heldFrames++;
            }
            else
            {
                PhysicsManager.HandleGravity(
                    e.StatsManager.CurrentStats.maxFallSpeed,
                    (e.CombatManager as FighterCombatManager).hitstunGravity,
                    PhysicsManager.GravityScale);
                if(Mathf.Abs(PhysicsManager.forceGravity.y) <= 0.001f)
                {
                    PhysicsManager.forceGravity.y = 0;
                }
            }

            /*
            if (gotOffset != Vector3.zero)
            {
                //e.cc.Motor.SetPosition(e.transform.position + gotOffset, false);
            }*/

            //float f = (((float)e.StateManager.CurrentStateFrame / (float)e.CombatManager.HitStun) * 10.0f);
            //(Manager as FighterManager).entityAnimator.SetFrame((int)f);

            if (CheckInterrupt() == false)
            {
                StateManager.IncrementFrame();
            }
        }

        public override bool CheckInterrupt()
        {
            FighterManager e = FighterManager;
            e.PhysicsManager.CheckIfGrounded();
            if (e.StateManager.CurrentStateFrame > e.CombatManager.HitStun)
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
            else if (e.PhysicsManager.IsGrounded == true)
            {
                e.StateManager.ChangeState((int)FighterStates.FLINCH_GROUND, e.StateManager.CurrentStateFrame, false);
                return true;
            }
            return false;
        }
    }
}