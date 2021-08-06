using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateIdle : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            PhysicsManager.forceGravity = Vector3.zero;
        }

        public override void OnUpdate()
        {
            PhysicsManager.ApplyMovementFriction();
            if ((Manager as FighterManager).LockedOn)
            {
                (Manager as FighterManager).RotateVisual((Manager as FighterManager).LockonForward, 10);
            }
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            PhysicsManager.CheckIfGrounded();
            if (PhysicsManager.IsGrounded == false)
            {
                StateManager.ChangeState((ushort)FighterStates.FALL);
                return true;
            }
            if (FighterManager.TryBlock())
            {
                return true;
            }
            if (FighterManager.TryAttack())
            {
                return true;
            }
            if (FighterManager.TryJump())
            {
                return true;
            }
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D((int)PlayerInputType.MOVEMENT, 0);
            if (mov.magnitude >= InputConstants.movementThreshold)
            {
                StateManager.ChangeState((ushort)FighterStates.WALK);
                return true;
            }
            return false;
        }
    }
}