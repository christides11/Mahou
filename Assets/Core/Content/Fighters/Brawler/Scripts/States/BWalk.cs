using Mahou.Content.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Core
{
    public class BWalk : FighterState
    {
        public override void OnUpdate()
        {
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(0, 0);
            (Manager as FighterManager).cc.Motor.SetPosition(Manager.transform.position + (new Vector3(mov.x, 0, mov.y) * 0.5f));

            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            Vector2 mov = (Manager.InputManager as FighterInputManager).GetAxis2D(0, 0);
            if(mov.magnitude <= 0.2f)
            {
                StateManager.ChangeState((ushort)BrawlerState.IDLE);
                return true;
            }
            return false;
        }
    }
}