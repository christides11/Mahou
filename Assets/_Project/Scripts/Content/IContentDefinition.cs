using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public abstract class IContentDefinition : ScriptableObject
    {
        public virtual string Identifier { get; set; } = "";
        public virtual string Name { get; }
        public virtual string Description { get; }
    }
}