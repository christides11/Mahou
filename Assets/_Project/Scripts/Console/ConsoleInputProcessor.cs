using Mahou.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mahou.Debugging
{
    public class ConsoleInputProcessor : MonoBehaviour
    {
        public GameManager gameManager;
        public ConsoleWindow consoleWindow;

        //ConsoleCommand unityVersionCommand;
        //public List<object> commandList;

        public void Awake()
        {
            /*
            unityVersionCommand = new ConsoleCommand("uv", "Prints the version of the Unity Engine the game is using.", "uv", () =>
            {
                ConsoleWindow.current.WriteLine(Application.unityVersion);
            });

            commandList = new List<object>()
            {
                unityVersionCommand
            };*/
        }

        List<MethodInfo> staticCommandMembers = new List<MethodInfo>();

        private void Start()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach(Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach(Type type in types)
                {
                    BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
                    MethodInfo[] methods = type.GetMethods(flags);

                    foreach(MethodInfo method in methods)
                    {
                        if(method.CustomAttributes.ToArray().Length > 0)
                        {
                            CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                            if(attribute != null)
                            {
                                staticCommandMembers.Add(method);
                            }
                        }
                    }
                }
            }
        }

        public async virtual Task Process(List<ConsoleInput> inputs)
        {
            foreach(ConsoleInput input in inputs)
            {
                await Process(input);
            }
        }

        public async virtual Task Process(ConsoleInput input)
        {
            if(input.input[0] == "help")
            {
                foreach(MethodInfo m in staticCommandMembers)
                {
                    CommandAttribute attribute = m.GetCustomAttribute<CommandAttribute>();

                    string callFormat = "";
                    callFormat += attribute.commandId + " ";
                    foreach(var para in m.GetParameters())
                    {
                        callFormat += para.ParameterType.ToString() + " ";
                    }
                    callFormat += "- " + attribute.commandDescrition;

                    consoleWindow.WriteLine(callFormat);
                }
            }

            foreach (MethodInfo m in staticCommandMembers)
            {
                CommandAttribute attribute = m.GetCustomAttribute<CommandAttribute>();

                // Check if it's this input.
                if(input.input[0] != attribute.commandId)
                {
                    continue;
                }
                if(m.GetParameters().Count() == 0 && input.input.Count()-1 == 0)
                {
                    m.Invoke(null, new object[0]);
                    return;
                }
                // Check if input has the same parameter count.
                if(input.input.Count()-1 < m.GetParameters().Count())
                {
                    continue;
                }

            }
        }
    }
}