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

        public float autoLinkForcePercentage = 1.0f;

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

        protected override bool CheckAttackNodeConditions(HnSF.Combat.MovesetAttackNode node)
        {
            if ((node as Mahou.Combat.MovesetAttackNode).lockonRequired == true && (manager as FighterManager).LockedOn == false)
            {
                return false;
            }
            return true;
        }

        public override int GetTeam()
        {
            return (int)team;
        }

        public AnimationCurve xCurvePosition;
        public AnimationCurve yCurvePosition;
        public AnimationCurve zCurvePosition;
        public AnimationCurve gravityCurve;
        public override HitReactionBase Hurt(HurtInfoBase hurtInfoBase)
        {
            FighterManager fManager = (FighterManager)manager;
            FighterPhysicsManager physicsManager = (FighterPhysicsManager)manager.PhysicsManager;
            FighterHurtboxManager hurtboxManager = manager.HurtboxManager as FighterHurtboxManager;
            HurtInfo hurtInfo = hurtInfoBase as HurtInfo;
            HitInfo hitInfo = hurtInfo.hitInfo as HitInfo;
            HitReactionBase HitReactionBase = new HitReactionBase();

            fManager.SetVisualRotation(-hurtInfo.forward);

            int indexOfHurtboxGroup = hurtboxManager.GetHurtboxDefinition().hurtboxGroups.IndexOf(hurtInfo.hurtboxGroupHit);
            if (hurtboxManager.hurtboxHitCount.ContainsKey(indexOfHurtboxGroup) == false)
            {
                hurtboxManager.hurtboxHitCount.Add(indexOfHurtboxGroup, 0);
            }
            hurtboxManager.hurtboxHitCount[indexOfHurtboxGroup] += 1;

            //HitReactionBase.reactionType = HitReactionBaseType.Hit;
            if(hurtInfo.hurtboxGroupHit.armor == ArmorType.PARRY)
            {
                return HitReactionBase;
            }
            // Check if the box can hit this entity.
            if (hitInfo.groundOnly && !physicsManager.IsGrounded
                || hitInfo.airOnly && physicsManager.IsGrounded)
            {
                //HitReactionBase.reactionType = HitReactionBaseType.Avoided;
                return HitReactionBase;
            }
            // Got hit, apply stun, damage, and forces.
            SetHitStop(hitInfo.hitstop);
            SetHitStun(hitInfo.hitstun);

            xCurvePosition = hitInfo.xCurvePosition;
            yCurvePosition = hitInfo.yCurvePosition;
            zCurvePosition = hitInfo.zCurvePosition;
            gravityCurve = hitInfo.gravityCurve;

            Vector3 startOffset = (hurtInfo.right * xCurvePosition.Evaluate(0))
                + (hurtInfo.forward * zCurvePosition.Evaluate(0))
                + (Vector3.up * yCurvePosition.Evaluate(0));

            fManager.cc.Motor.SetPosition(transform.position + startOffset);
            /*
            // Convert forces the attacker-based forward direction.
            switch (hitInfo.forceType)
            {
                case HitboxForceType.SET:
                    Vector3 forces = (hitInfo.opponentForce.x * hurtInfo.right) + (hitInfo.opponentForce.z * hurtInfo.forward);
                    physicsManager.forceGravity.y = hitInfo.opponentForce.y;
                    physicsManager.forceMovement = forces;
                    break;
                case HitboxForceType.PULL:
                    Vector3 pullDir = Vector3.ClampMagnitude((hurtInfo.center - transform.position) * hitInfo.opponentForceMultiplier, hitInfo.opponentMaxMagnitude);
                    if(pullDir.magnitude < hitInfo.opponentMinMagnitude)
                    {
                        pullDir = (hurtInfo.center - transform.position).normalized * hitInfo.opponentMinMagnitude;
                    }
                    if (hitInfo.forceIncludeYForce)
                    {
                        physicsManager.forceGravity.y = pullDir.y;
                    }
                    pullDir.y = 0;
                    physicsManager.forceMovement = pullDir;
                    break;
                case HitboxForceType.PUSH:
                    Vector3 pushDir = Vector3.ClampMagnitude((transform.position - hurtInfo.center) * hitInfo.opponentForceMultiplier, hitInfo.opponentMaxMagnitude);
                    if (pushDir.magnitude < hitInfo.opponentMinMagnitude)
                    {
                        pushDir = (transform.position - hurtInfo.center).normalized * hitInfo.opponentMinMagnitude;
                    }
                    if (hitInfo.forceIncludeYForce)
                    {
                        physicsManager.forceGravity.y = pushDir.y;
                    }
                    pushDir.y = 0;
                    physicsManager.forceMovement = pushDir;
                    break;
            }

            if (hitInfo.autoLink)
            {
                physicsManager.forceMovement.x += hurtInfo.attackerVelocity.x * autoLinkForcePercentage;
                physicsManager.forceMovement.z += hurtInfo.attackerVelocity.z * autoLinkForcePercentage;
            }

            if (physicsManager.forceGravity.y > 0)
            {
                physicsManager.SetGrounded(false);
            }*/

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
            return HitReactionBase;
        }

        protected override bool CheckStickDirection(InputDefinition sequenceInput, uint framesBack)
        {
            Vector2 stickDir = manager.InputManager.GetAxis2D((int)PlayerInputType.MOVEMENT, framesBack);
            if (stickDir.magnitude < InputConstants.movementThreshold)
            {
                return false;
            }

            Vector3 wantedDir = manager.GetVisualBasedDirection(new Vector3(sequenceInput.stickDirection.x, 0, sequenceInput.stickDirection.y)).normalized;
            Vector3 currentDirection = manager.GetMovementVector(stickDir.x, stickDir.y).normalized;

            if (Vector3.Dot(wantedDir, currentDirection) >= sequenceInput.directionDeviation)
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