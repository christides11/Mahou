using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BAirDash : FighterState
    {
        Vector3 oldPos;
        public override void Initialize()
        {
            base.Initialize();
            oldPos = Manager.transform.position;
        }

        public override void OnUpdate()
        {
            if(StateManager.CurrentStateFrame == Stats.baseStats.airDashPreFrames)
            {
                Vector3 translatedMovement = (Manager as BrawlerManager).GetMovementVector();
                translatedMovement.y = 0;
                if(translatedMovement.magnitude < InputConstants.movementThreshold)
                {
                    translatedMovement = Manager.GetMovementVector(0, 1);
                }

                PhysicsManager.forceGravity = Vector3.zero;
                PhysicsManager.forceMovement = translatedMovement.normalized * Stats.baseStats.airDashInitVelo;
            }

            if(StateManager.CurrentStateFrame >= Stats.baseStats.airDashFrictionAfter)
            {
                PhysicsManager.ApplyMovementFriction(Stats.baseStats.airDashFriction);
            }

            if(StateManager.CurrentStateFrame >= Stats.baseStats.airDashGravityAfter)
            {
                PhysicsManager.HandleGravity();
            }

            if(StateManager.CurrentStateFrame >= Stats.baseStats.airDashGravityAfter)
            {
                Debug.DrawLine(oldPos, Manager.transform.position, Color.red, 2.0f);
            }else if(StateManager.CurrentStateFrame >= Stats.baseStats.airDashFrictionAfter)
            {
                Debug.DrawLine(oldPos, Manager.transform.position, Color.blue, 2.0f);
            }
            else
            {
                Debug.DrawLine(oldPos, Manager.transform.position, Color.green, 2.0f);
            }

            oldPos = Manager.transform.position;
            CheckInterrupt();
            StateManager.IncrementFrame();
        }

        public override bool CheckInterrupt()
        {
            if(StateManager.CurrentStateFrame >= Stats.baseStats.airDashLength)
            {
                StateManager.ChangeState((ushort)FighterStates.FALL);
                return true;
            }

            if (Manager.IsGrounded)
            {
                StateManager.ChangeState((ushort)FighterStates.IDLE);
                return true;
            }
            return false;
        }
    }
}