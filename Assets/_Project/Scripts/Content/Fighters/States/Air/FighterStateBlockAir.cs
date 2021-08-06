using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateBlockAir : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            PhysicsManager.forceGravity = Vector3.zero;
            (Manager.CombatManager as FighterCombatManager).blockState = BlockStateType.AIR;
        }

        public override void OnUpdate()
        {
            PhysicsManager.ApplyMovementFriction();

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
                StateManager.ChangeState((ushort)FighterStates.FALL);
                return true;
            }

            PhysicsManager.CheckIfGrounded();
            if (PhysicsManager.IsGrounded == true)
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