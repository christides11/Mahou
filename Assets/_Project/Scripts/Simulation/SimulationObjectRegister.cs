using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Mahou.Simulation
{
    public class SimulationObjectRegister : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            SimulationManagerBase.instance.RegisterSimulationObject(netIdentity);
        }

        public override void OnStartClient()
        {
            if (NetworkServer.active)
            {
                return;
            }
            SimulationManagerBase.instance.RegisterSimulationObject(netIdentity);
        }
    }
}