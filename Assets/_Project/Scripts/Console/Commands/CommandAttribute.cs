using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Debugging
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string commandId;
        public string commandDescrition;

        public CommandAttribute(string id, string description)
        {
            commandId = id;
            commandDescrition = description;
        }
    }
}