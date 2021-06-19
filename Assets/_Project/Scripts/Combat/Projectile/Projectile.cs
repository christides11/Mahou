using Mahou.Simulation;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class Projectile : NetworkBehaviour, ISimObject
    {
        public bool ObjectEnabled { get; protected set; } = true;

        public void Enable()
        {
            ObjectEnabled = true;
        }

        public void Disable()
        {
            ObjectEnabled = false;
        }

        public void SimUpdate()
        {

        }

        public void SimLateUpdate()
        {

        }

        public ISimState GetSimState()
        {
            return null;
        }

        public void ApplySimState(ISimState state)
        {

        }
    }
}