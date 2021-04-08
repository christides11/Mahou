using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterPhysicsManager : CAF.Fighters.FighterPhysicsManager3D
    {
        protected FighterManager Manager { get { return (FighterManager)manager; } }

        public override void Tick()
        {
            Manager.cc.SetMovement(forceMovement + forcePushbox, forceDamage, forceGravity);
            forcePushbox = Vector3.zero;
        }

        public override void Freeze()
        {
            Manager.cc.SetMovement(Vector3.zero, Vector3.zero);
        }

        public override Vector3 GetOverallForce()
        {
            return forceMovement + forceGravity + forceDamage;
        }
    }
}