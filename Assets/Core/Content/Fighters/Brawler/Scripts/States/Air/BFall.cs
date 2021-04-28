using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BFall : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();

            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.ApplyMovement(es.baseStats.airAcceleration, es.baseStats.maxAirSpeed, es.baseStats.airDeceleration);
            PhysicsManager.HandleGravity();
        }

        public override void OnUpdate()
        {
            FighterStatsManager es = (Manager as FighterManager).StatsManager;
            PhysicsManager.ApplyMovement(es.baseStats.airAcceleration, es.baseStats.maxAirSpeed, es.baseStats.airDeceleration);
            PhysicsManager.HandleGravity();

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            if (FighterManager.TryJump())
            {
                return true;
            }
            if (InputManager.GetButton(Input.Action.Dash, 0).firstPress)
            {
                StateManager.ChangeState((ushort)FighterStates.AIR_DASH);
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