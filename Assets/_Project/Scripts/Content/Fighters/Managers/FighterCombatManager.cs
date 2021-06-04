using HnSF.Combat;
using HnSF.Input;
using Mahou.Combat;
using Mahou.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HitInfo = Mahou.Combat.HitInfo;

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
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)manager.PhysicsManager;
            HurtInfo hurtInfo = hurtInfoBase as HurtInfo;
            HitInfo hitInfo = hurtInfo.hitInfo as HitInfo;
            HitReaction hitReaction = new HitReaction();
            hitReaction.reactionType = HitReactionType.Hit;
            if(hurtInfo.hurtboxGroupHit.armor == ArmorType.PARRY)
            {
                hitReaction.reactionType = HitReactionType.Blocked;
                return hitReaction;
            }
            // Check if the box can hit this entity.
            if (hitInfo.groundOnly && !physicsManager.IsGrounded
                || hitInfo.airOnly && physicsManager.IsGrounded)
            {
                hitReaction.reactionType = HitReactionType.Avoided;
                return hitReaction;
            }
            // Got hit, apply stun, damage, and forces.
            //LastHitBy = hInfo;
            SetHitStop(hitInfo.hitstop);
            SetHitStun(hitInfo.hitstun);

            // Convert forces the attacker-based forward direction.
            switch (hitInfo.forceType)
            {
                case HitboxForceType.SET:
                    Vector3 forces = (hitInfo.opponentForce.x * hurtInfo.right) + (hitInfo.opponentForce.z * hurtInfo.forward);
                    physicsManager.forceGravity.y = forces.y;
                    forces.y = 0;
                    physicsManager.forceMovement = forces;
                    break;
            }

            if (physicsManager.forceGravity.y > 0)
            {
                physicsManager.SetGrounded(false);
            }

            // Change into the correct state.
            if (hitInfo.groundBounces && physicsManager.IsGrounded)
            {
                manager.StateManager.ChangeState((int)FighterStates.GROUND_BOUNCE);
            }
            else if (hitInfo.causesTumble)
            {
                manager.StateManager.ChangeState((int)FighterStates.TUMBLE);
            }
            else
            {
                manager.StateManager.ChangeState((ushort)(physicsManager.IsGrounded ? FighterStates.FLINCH_GROUND : FighterStates.FLINCH_AIR));
            }
            return hitReaction;
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