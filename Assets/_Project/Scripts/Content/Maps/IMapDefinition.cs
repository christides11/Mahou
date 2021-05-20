using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public interface IMapDefinition
    {
        public string Identifier { get; }
        public string Name { get; }
        public List<string> SceneNames { get; }
        public string Description { get; }
        public bool Selectable { get; }

        UniTask LoadScene();
    }
}