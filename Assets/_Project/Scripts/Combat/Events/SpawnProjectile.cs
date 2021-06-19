using UnityEngine;
using System.Collections.Generic;
using HnSF.Combat;
using Mahou;
using Mahou.Content.Fighters;
using Mahou.Simulation;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mahou.Combat.Events
{
    public class SpawnProjectile : AttackEvent
    {
        public override string GetName()
        {
            return "Spawn Projectile";
        }

        public AssetIdentifier projectilePrefab;
        public Vector3 offset;

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            SimulationCreationManager.Create(projectilePrefab, controller.visual.transform.position + offset, Quaternion.identity);
            return AttackEventReturnType.NONE;
        }
    }
}