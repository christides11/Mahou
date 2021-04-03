using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Debugging
{
    public class ExtConsoleCommands
    {
        [Command("unity-version", "Prints the current unity version.")]
        public static void PrintUnityVersion()
        {
            ConsoleWindow.current.WriteLine(Application.unityVersion);
        }

        [Command("version", "Prints the current game version.")]
        public static void PrintGameVersion()
        {
            ConsoleWindow.current.WriteLine(Application.version);
        }
    }
}