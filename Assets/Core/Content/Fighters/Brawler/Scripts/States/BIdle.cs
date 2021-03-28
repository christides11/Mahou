using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BIdle : FighterState
    {
        public override void OnUpdate()
        {
            base.OnUpdate();

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(0, 0);
            if (mov.magnitude > 0.2f)
            {
                StateManager.ChangeState((ushort)BrawlerState.WALK);
                return true;
            }
            return false;
        }
    }
}