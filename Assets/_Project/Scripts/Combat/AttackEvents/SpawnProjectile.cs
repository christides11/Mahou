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

namespace Mahou.Combat.AttackEvents
{
    public class SpawnProjectile : AttackEvent
    {
        public override string GetName()
        {
            return "Spawn Projectile";
        }

        public AssetIdentifier projectilePrefab;
        public bool spawnAtEnemy;
        public float maxDistance;
        public Vector3 offset;

        public override AttackEventReturnType Evaluate(int frame, int endFrame,
            HnSF.Fighters.FighterBase controller, AttackEventVariables variables)
        {
            FighterManager fm = (controller as Mahou.Content.Fighters.FighterManager);
            Vector3 spawnPosition = controller.transform.position 
                + (fm.visual.transform.forward * offset.z)
                + (fm.visual.transform.right * offset.x)
                + (fm.visual.transform.up * offset.y);

            if (spawnAtEnemy && fm.LockedOn)
            {
                GameObject lockonTarget = fm.LockonTarget;
                if ( lockonTarget != null && Vector3.Distance(controller.transform.position, lockonTarget.transform.position) <= maxDistance)
                {
                    spawnPosition = lockonTarget.transform.position;
                }
            }
            GameObject projectile = SimulationCreationManager.Create(projectilePrefab, spawnPosition, fm.visual.transform.rotation);
            projectile.GetComponent<Projectile>().owner = fm.netid;
            return AttackEventReturnType.NONE;
        }
    }
}