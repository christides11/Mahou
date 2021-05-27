using HnSF.Combat;
using HnSF.Input;
using Mahou.Combat;
using Mahou.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterCombatManager : HnSF.Fighters.FighterCombatManager
    {
        public override HnSF.Combat.MovesetDefinition CurrentMoveset { get { return (manager as FighterManager).movesets[currentMoveset]; } }

        public TeamTypes team;

        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
            hitboxManager = GetComponent<FighterHitboxManager>();
        }

        public override int GetMovesetCount()
        {
            return (manager as FighterManager).movesets.Length;
        }

        public override HnSF.Combat.MovesetDefinition GetMoveset(int index)
        {
            return (manager as FighterManager).movesets[index];
        }

        public override int GetTeam()
        {
            return (int)team;
        }

        public override HitReaction Hurt(HurtInfoBase hurtInfoBase)
        {
            if (SimulationManagerBase.IsRollbackFrame == false)
            {
                Debug.Log("Was hit.");
            }
            return base.Hurt(hurtInfoBase);
        }

        protected override bool CheckStickDirection(InputDefinition sequenceInput, uint framesBack)
        {
            Vector2 stickDir = manager.InputManager.GetAxis2D(Input.Action.Movement_X, framesBack);
            if (stickDir.magnitude < InputConstants.movementThreshold)
            {
                return false;
            }

            if (Vector2.Dot(stickDir, sequenceInput.stickDirection) >= sequenceInput.directionDeviation)
            {
                return true;
            }
            return false;
        }

        public virtual void ApplySimState(int currentMoveset, int currentAttackMoveset, int currentAttackNode)
        {
            this.currentMoveset = currentMoveset;
            this.currentAttackMoveset = currentAttackMoveset;
            this.currentAttackNode = currentAttackNode;
        }
    }
}