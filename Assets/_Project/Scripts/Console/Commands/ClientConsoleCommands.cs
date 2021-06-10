using Mahou.Managers;
using Mahou.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Debugging
{
    public class ClientConsoleCommands
    {
        [Command("join", "Joins a server.")]
        public static void JoinServer(string ip)
        {
            ConsoleWindow.current.WriteLine($"Joining {ip}");
            NetworkManager networkManager = GameManager.current.NetworkManager;
            networkManager.JoinGame(ip);
        }

        [Command("inputdelay", "Sets the client's input delay.")]
        public static void SetInputDelay(int newInputDelay)
        {
            if(newInputDelay < 0)
            {
                return;
            }
            if (Simulation.SimulationManagerBase.instance == null)
            {
                return;
            }
            /*Simulation.ClientSimulationManager ssm = Simulation.SimulationManagerBase.instance as Simulation.ClientSimulationManager;
            if (ssm == null)
            {
                return;
            }
            ssm.SetInputDelay(newInputDelay);*/
            Simulation.SimulationManagerBase.instance.RequestInputDelayChange(newInputDelay);
            ConsoleWindow.current.WriteLine($"Input delay set to {newInputDelay}.");
        }
    }
}