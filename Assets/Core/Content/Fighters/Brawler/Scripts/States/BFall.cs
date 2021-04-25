using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BFall : FighterState
    {
        public override void OnUpdate()
        {
            FighterStats es = (Manager as FighterManager).Stats;
            PhysicsManager.ApplyMovement(es.airAcceleration, es.maxAirSpeed, es.airDeceleration);
            PhysicsManager.HandleGravity();

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (InputManager.GetButton(Input.Action.Dash, 0).firstPress)
            {
                StateManager.ChangeState((ushort)BrawlerState.AIR_DASH);
                return true;
            }
            if (Manager.IsGrounded)
            {
                StateManager.ChangeState((ushort)BrawlerState.IDLE);
                return true;
            }
            return false;
        }
    }
}