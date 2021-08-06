using Mahou.Managers;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Debugging
{
    public class HostConsoleCommands
    {
        [Command("hostroom", "Host a room with the given parameters.")]
        public static void HostRoom()
        {
            NetworkManager.singleton.StartHost();
        }

        [Command("quickhost", "Host a room with the given parameters.")]
        public static void QuickHost(string character, string gamemode, string[] content)
        {
            List<Content.ModObjectReference> requiredContent = new List<Content.ModObjectReference>();
            for (int i = 0; i < content.Length; i++)
            {
                requiredContent.Add(new Content.ModObjectReference(content[i]));
            }
            LobbyManager.current.SetLobbySettings(new LobbySettings() { selectedGamemode = new Content.ModObjectReference(gamemode), requiredContent = requiredContent });

            NetworkManager.singleton.StartHost();

            _ = LobbyManager.current.InitializeMatch();
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