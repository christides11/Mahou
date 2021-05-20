using Mahou.Content.Fighters;
using UnityEngine;

namespace Mahou.Core
{
    public class BEnemyStep : FighterState
    {
        public override void Initialize()
        {
            base.Initialize();
            //storedMovementForce = PhysicsManager.forceMovement;
        }

        public override void OnUpdate()
        {
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            return false;
        }
    }
}