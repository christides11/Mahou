using Mahou.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Debugging
{
    public class HostConsoleCommands
    {
        [Command("host", "Host a room with the given parameters.")]
        public static void HostRoom(string gamemode, string map, string battle)
        {
            /*
            GameManager.current.LobbyManager.HostGame(
                new LobbySettings(
                    new Content.ModObjectReference(gamemode), 
                    new Content.ModObjectReference(map),
                    new Content.ModObjectReference(battle))
                );*/
        }

        [Command("host", "Host a room with the given parameters.")]
        public static void HostRoom(string gamemode, string battle)
        {

        }

        [Command("closeroom", "Closes the current room.")]
        public static void CloseRoom()
        {

        }

        [Command("statesendrate", "Sets the send rate of the world state (server only).")]
        public static void SetWorldStateSendRate(int tickRate)
        {
            if(tickRate < 1 || tickRate > 60)
            {
                return;
            }
            if(Simulation.SimulationManagerBase.instance == null)
            {
                return;
            }
            Simulation.ServerSimulationManager ssm = Simulation.SimulationManagerBase.instance as Simulation.ServerSimulationManager;
            if(ssm == null)
            {
                return;
            }
            ssm.SetWorldStateBroadcastTimer((float)tickRate);
            ConsoleWindow.current.WriteLine($"Changed send rate to {tickRate} (State sent every {1.0f/(float)tickRate} second(s)).");
        }
    }
}