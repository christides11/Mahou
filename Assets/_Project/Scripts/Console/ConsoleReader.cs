using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mahou.Debugging
{
    public class ConsoleReader : MonoBehaviour
    {
        [SerializeField] private ConsoleInputProcessor inputProcessor;

        public async Task ReadCommandLine()
        {
            await Convert(System.Environment.CommandLine);
        }

        public async Task Convert(string input)
        {
            List<ConsoleInput> inputs = new List<ConsoleInput>();

            string[] inputLines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < inputLines.Length; i++)
            {
                inputs.Add(new ConsoleInput(inputLines[i].Split(' ')));
            }

            await inputProcessor.Process(inputs);
        }
    }
}