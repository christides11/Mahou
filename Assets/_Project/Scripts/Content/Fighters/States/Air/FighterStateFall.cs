using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateFall : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();

            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.HandleMovement(es.CurrentStats.airBaseAccel, es.CurrentStats.airAccel, es.CurrentStats.airDeceleration,
                es.CurrentStats.maxAirSpeed, es.CurrentStats.accelFromDotProduct);
            PhysicsManager.HandleGravity();
        }

        public override void OnUpdate()
        {
            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.HandleMovement(es.CurrentStats.airBaseAccel, es.CurrentStats.airAccel, es.CurrentStats.airDeceleration,
                es.CurrentStats.maxAirSpeed, es.CurrentStats.accelFromDotProduct);
            PhysicsManager.HandleGravity();

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryAttack())
            {
                return true;
            }
            if (FighterManager.TryJump())
            {
                return true;
            }
            if (InputManager.GetButton((int)PlayerInputType.DASH, 0).firstPress)
            {
                StateManager.ChangeState((ushort)FighterStates.AIR_DASH);
                return true;
            }
            if (Manager.PhysicsManager.IsGrounded)
            {
                Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D((int)PlayerInputType.MOVEMENT, 0);
                if (mov.magnitude >= InputConstants.movementThreshold)
                {
                    StateManager.ChangeState((ushort)FighterStates.WALK);
                    return true;
                }
                else
                {
                    StateManager.ChangeState((ushort)FighterStates.IDLE);
                }
                return true;
            }
            return false;
        }
    }
}