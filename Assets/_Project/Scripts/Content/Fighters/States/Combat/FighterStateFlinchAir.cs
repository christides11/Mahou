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
            //(Manager as FighterManager).entityAnimator.PlayAnimation((Manager as FighterManager).GetAnimationClip("hurt"));
        }

        public override void OnUpdate()
        {
            FighterManager e = FighterManager;
            FighterCombatManager cm = (FighterCombatManager)e.CombatManager;
            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("flinch-air"),
                StateManager.CurrentStateFrame);

            /*
            float fallSpeedMulti = cm.gravityCurve.Evaluate((float)e.StateManager.CurrentStateFrame / (float)cm.HitStun);
            PhysicsManager.HandleGravity(
                e.StatsManager.CurrentStats.maxFallSpeed,
                e.StatsManager.CurrentStats.gravity * fallSpeedMulti,
                PhysicsManager.GravityScale);*/

            Vector3 gotOffset = Vector3.zero;
            float yFrameOffset = cm.yCurvePosition.Evaluate((float)e.StateManager.CurrentStateFrame / (float)cm.HitStun)
                - cm.yCurvePosition.Evaluate((float)((int)e.StateManager.CurrentStateFrame - 1) / (float)cm.HitStun);
            float zFrameOffset = cm.zCurvePosition.Evaluate((float)e.StateManager.CurrentStateFrame / (float)cm.HitStun)
                 - cm.zCurvePosition.Evaluate((float)((int)e.StateManager.CurrentStateFrame - 1) / (float)cm.HitStun);

            if (yFrameOffset != 0)
            {
                gotOffset += yFrameOffset * Vector3.up;
            }
            if (zFrameOffset != 0)
            {
                gotOffset += -zFrameOffset * e.visual.transform.forward;
            }

            gotOffset /= Time.fixedDeltaTime;
            PhysicsManager.forceMovement = Vector3.zero;
            gotOffset.y = Mathf.Lerp(gotOffset.y, -e.StatsManager.CurrentStats.gravity, cm.gravityCurve.Evaluate((float)e.StateManager.CurrentStateFrame / (float)cm.HitStun));
            if (gotOffset != Vector3.zero)
            {
                PhysicsManager.forceGravity.y = gotOffset.y;
                gotOffset.y = 0;
                PhysicsManager.forceMovement = gotOffset;   
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