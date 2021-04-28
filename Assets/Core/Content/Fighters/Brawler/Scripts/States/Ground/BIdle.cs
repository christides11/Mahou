using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BIdle : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            FighterManager.ResetGroundOptions();
        }

        public override void OnUpdate()
        {
            PhysicsManager.ApplyMovementFriction();
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            PhysicsManager.CheckIfGrounded();

            if (FighterManager.TryJump())
            {
                return true;
            }
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(Input.Action.Movement_X, 0);
            if (mov.magnitude > 0.2f)
            {
                StateManager.ChangeState((ushort)FighterStates.WALK);
                return true;
            }
            return false;
        }
    }
}