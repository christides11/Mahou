using KinematicCharacterController;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HnSF.Combat.HitboxManager;

namespace Mahou.Simulation
{
    [System.Serializable]
    public class PlayerSimState : ISimState
    {
        public NetworkIdentity netID;

        public Vector3 visualRotation;

        public Vector3 forceMovement;
        public Vector3 forceGravity;
        public KinematicCharacterMotorState motorState;

        public ushort mainState;
        public uint mainStateFrame;
        public int currentJump;

        public bool isGrounded;
        public bool jumpHold;

        public bool lockedOn;
        public Vector3 lockonForward;
        public GameObject lockOnTarget;

        // Input Manager
        public uint inputBufferTick;

        // Combat Manager
        public int currentChargeLevel;
        public int currentChargeLevelCharge;

        public int currentMoveset;
        public int currentAttackMoveset;
        public int currentAttackNode;

        public int hitstun;
        public int hitstop;

        // Hitbox Manager
        public Dictionary<int, IDGroupCollisionInfo> collidedIHurtables;

        // Hurtbox Manager
        public Dictionary<int, int> hurtboxHitCount;

        // State
        public PlayerStateSimState stateSimState;

        public override Guid GetGUID()
        {
            return StaticGetGUID();
        }

        public static new System.Guid StaticGetGUID()
        {
            return new System.Guid("34102bae-80b6-4e86-927e-c056026131e2");
        }
    }
}