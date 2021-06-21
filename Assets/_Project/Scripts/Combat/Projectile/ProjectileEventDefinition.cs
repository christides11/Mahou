using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    [System.Serializable]
    public class ProjectileEventDefinition
    {
        public bool active = true;
        public string nickname;
        public int startFrame = 1;
        public int endFrame = 1;
        [SerializeReference] public ProjectileEvent projectileEvent;
    }
}