using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    [CreateAssetMenu(fileName = "ProjectileDefinition", menuName = "Mahou/Combat/Projectile Definition")]
    public class ProjectileDefinition : ScriptableObject
    {
        [Header("General")]
        public string projectileName;
        public int length = 1;
        public bool scriptOverride = false;

        [Header("Groups")]
        [SerializeReference] public List<HitboxGroup> hitboxGroups = new List<HitboxGroup>();
        [SerializeReference] public List<ProjectileEventDefinition> events = new List<ProjectileEventDefinition>();
    }
}