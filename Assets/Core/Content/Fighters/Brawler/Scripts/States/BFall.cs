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
            (Manager.PhysicsManager as FighterPhysicsManager).HandleGravity();

            CheckInterrupt();
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