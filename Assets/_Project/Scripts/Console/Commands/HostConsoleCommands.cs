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
            GameManager.current.LobbyManager.HostGame(
                new LobbySettings(
                    new Content.ModObjectReference(gamemode), 
                    new Content.ModObjectReference(map),
                    new Content.ModObjectReference(battle))
                );
        }

        [Command("host", "Host a room with the given parameters.")]
        public static void HostRoom(string gamemode, string battle)
        {

        }

        [Command("closeroom", "Closes the current room.")]
        public static void CloseRoom()
        {

        }
    }
}