using HnSF.Combat;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.AttackEvents
{
    public class ApplyGravity : HnSF.Combat.AttackEvent
    {
        public AnimationCurve gravityCurve = new AnimationCurve();

        public AnimationCurve gravityScaleCurve = new AnimationCurve();

        public AnimationCurve maxFallSpeedCurve = new AnimationCurve();

        public override string GetName()
        {
            return "Apply Gravity";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterStatsManager statsManager = (controller as FighterManager).StatsManager;
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)controller.PhysicsManager;
            if (physicsManager.IsGrounded)
            {
                physicsManager.forceGravity = Vector3.zero;
                return AttackEventReturnType.NONE;
            }
            float percent = (float)frame / (float)endFrame;

            float gravity = statsManager.CurrentStats.gravity.GetCurrentValue();
            float gravityScale = physicsManager.GravityScale;
            float maxFallSpeed = statsManager.CurrentStats.maxFallSpeed.GetCurrentValue();

            physicsManager.HandleGravity(
                maxFallSpeed * maxFallSpeedCurve.Evaluate(percent), 
                gravity * gravityCurve.Evaluate(percent),
                gravityScale * gravityScaleCurve.Evaluate(percent)
                );
            return AttackEventReturnType.NONE;
        }
    }
}