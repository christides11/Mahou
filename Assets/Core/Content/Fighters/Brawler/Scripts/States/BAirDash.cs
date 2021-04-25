using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BAirDash : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnUpdate()
        {
            if(StateManager.CurrentStateFrame == Stats.airDashPreFrames)
            {
                Vector3 translatedMovement = (Manager as BrawlerManager).GetMovementVector();
                translatedMovement.y = 0;
                if(translatedMovement.magnitude < InputConstants.movementThreshold)
                {
                    translatedMovement = Manager.GetMovementVector(0, 1);
                }

                PhysicsManager.forceGravity = Vector3.zero;
                PhysicsManager.forceMovement = translatedMovement.normalized * Stats.airDashInitVelo;
            }

            if(StateManager.CurrentStateFrame >= Stats.airDashFrictionAfter)
            {
                PhysicsManager.ApplyMovementFriction(Stats.airDashFriction);
            }

            if(StateManager.CurrentStateFrame >= Stats.airDashGravityAfter)
            {
                PhysicsManager.HandleGravity();
            }

            CheckInterrupt();
            StateManager.IncrementFrame();
        }

        public override bool CheckInterrupt()
        {
            if (Manager.IsGrounded)
            {
                StateManager.ChangeState((ushort)BrawlerState.IDLE);
                return true;
            }
            return false;
        }
    }
}