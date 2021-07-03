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
            FighterCombatManager cm = (FighterCombatManager)e.CombatManager;
            (FighterManager.HurtboxManager as FighterHurtboxManager).CreateHurtboxes(
                (FighterManager.CombatManager.CurrentMoveset as MovesetDefinition).hurtboxCollection.GetHurtbox("flinch"),
                StateManager.CurrentStateFrame);

            Vector3 gotOffset = Vector3.zero;
            float yFrameOffset = cm.yCurvePosition.Evaluate((float)e.StateManager.CurrentStateFrame / (float)cm.HitStun) 
                - cm.yCurvePosition.Evaluate((float)((int)e.StateManager.CurrentStateFrame - 1) / (float)cm.HitStun);
            float zFrameOffset = cm.zCurvePosition.Evaluate((float)e.StateManager.CurrentStateFrame / (float)cm.HitStun)
                 - cm.zCurvePosition.Evaluate((float)((int)e.StateManager.CurrentStateFrame - 1) / (float)cm.HitStun);

            if (yFrameOffset != 0)
            {
                gotOffset += yFrameOffset * Vector3.up;
            }
            if(zFrameOffset != 0)
            {
                gotOffset += -zFrameOffset * e.visual.transform.forward;
            }

            if (gotOffset != Vector3.zero)
            {
                e.cc.Motor.SetPosition(e.transform.position + gotOffset, false);
            }

            e.StateManager.IncrementFrame();

            //float f = (((float)e.StateManager.CurrentStateFrame / (float)e.CombatManager.HitStun) * 10.0f);
            //(Manager as FighterManager).entityAnimator.SetFrame((int)f);

            CheckInterrupt();
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
            else if (e.PhysicsManager.IsGrounded == false)
            {
                e.StateManager.ChangeState((int)FighterStates.FLINCH_AIR, e.StateManager.CurrentStateFrame, false);
                return true;
            }
            return false;
        }
    }
}