using HnSF.Combat;
using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat.AttackEvents
{
    public class ForceCurve : HnSF.Combat.AttackEvent
    {
        public bool applyXZForce;
        public bool applyYForce;

        public AnimationCurve xCurve;
        public AnimationCurve zCurve;
        public AnimationCurve yCurve;
        public AnimationCurve gravityCurve;

        public override string GetName()
        {
            return "Force Curve";
        }

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterManager e = (FighterManager)controller;
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)controller.PhysicsManager;

            Vector3 gotOffset = Vector3.zero;
            float xFrameOffset = xCurve.Evaluate((float)frame / (float)endFrame) - xCurve.Evaluate((float)(frame-1) / (float)endFrame);
            float zFrameOffset = zCurve.Evaluate((float)frame / (float)endFrame) - zCurve.Evaluate((float)(frame - 1) / (float)endFrame);
            float yFrameOffset = yCurve.Evaluate((float)frame / (float)endFrame) - yCurve.Evaluate((float)(frame-1) / (float)endFrame);

            if(xFrameOffset != 0)
            {
                gotOffset += xFrameOffset * e.visual.transform.right;
            }
            if (yFrameOffset != 0)
            {
                gotOffset += yFrameOffset * Vector3.up;
            }
            if (zFrameOffset != 0)
            {
                gotOffset += zFrameOffset * e.visual.transform.forward;
            }

            gotOffset /= Time.fixedDeltaTime;
            if (applyYForce)
            {
                physicsManager.forceGravity = Vector3.zero;
            }
            if (applyXZForce)
            {
                physicsManager.forceMovement = Vector3.zero;
            }
            gotOffset.y = Mathf.Lerp(gotOffset.y, -e.StatsManager.CurrentStats.gravity, gravityCurve.Evaluate((float)frame / (float)endFrame));
            if (gotOffset != Vector3.zero)
            {
                if (applyYForce)
                {
                    physicsManager.forceGravity.y = gotOffset.y;
                }
                gotOffset.y = 0;
                if (applyXZForce)
                {
                    physicsManager.forceMovement = gotOffset;
                }
            }
            return AttackEventReturnType.NONE;
        }
    }
}