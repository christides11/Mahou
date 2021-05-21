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
    }
}