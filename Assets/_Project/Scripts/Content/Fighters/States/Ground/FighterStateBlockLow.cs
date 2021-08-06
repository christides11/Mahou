using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateBlockLow : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            PhysicsManager.forceGravity = Vector3.zero;
            (Manager.CombatManager as FighterCombatManager).blockState = BlockStateType.LOW;
            Debug.Log("Low Blocking");
        }

        public override void OnUpdate()
        {
            PhysicsManager.ApplyMovementFriction();

            Vector3 movement = FighterManager.GetMovementVector();
            movement.y = 0;
            if (movement.magnitude >= InputConstants.movementThreshold)
            {
                FighterManager.SetVisualRotation(movement.normalized);
            }

            if (CheckInterrupt() == false)
            {
                StateManager.IncrementFrame();
            }
        }

        public override void OnInterrupted()
        {
            (Manager.CombatManager as FighterCombatManager).blockState = BlockStateType.NONE;
        }

        public override bool CheckInterrupt()
        {
            if ((Manager.InputManager as FighterInputManager).GetButton((int)PlayerInputType.BLOCK).isDown == false)
            {
                StateManager.ChangeState((ushort)FighterStates.IDLE);
                return true;
            }

            PhysicsManager.CheckIfGrounded();
            if (PhysicsManager.IsGrounded == false)
            {
                StateManager.ChangeState((ushort)FighterStates.BLOCK_AIR);
                return true;
            }
            if ((Manager.InputManager as FighterInputManager).GetButton((int)PlayerInputType.LIGHT_ATTACK).firstPress == true)
            {
                StateManager.ChangeState((ushort)FighterStates.BLOCK_HIGH);
                return true;
            }
            if (FighterManager.TryJump())
            {
                return true;
            }
            return false;
        }
    }
}