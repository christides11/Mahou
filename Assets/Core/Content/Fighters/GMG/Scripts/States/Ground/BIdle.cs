using Mahou.Content.Fighters;
using UnityEngine;

namespace Mahou.Core
{
    public class BIdle : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            PhysicsManager.forceGravity = Vector3.zero;
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
            if (mov.magnitude >= InputConstants.movementThreshold)
            {
                StateManager.ChangeState((ushort)FighterStates.WALK);
                return true;
            }
            return false;
        }
    }
}