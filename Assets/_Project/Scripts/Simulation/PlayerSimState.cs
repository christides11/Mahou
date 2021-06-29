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
        public float visualRotation;

        public Vector3 forceMovementGravity;
        public KinematicCharacterMotorState motorState;

        public ushort mainState;
        public uint mainStateFrame;
        public byte currentJump;

        public bool isGrounded;
        public bool jumpHold;

        public bool lockedOn;
        public Vector3 lockonForward;
        public GameObject lockOnTarget;

        // Input Manager
        public uint inputBufferTick;

        // Combat Manager
        public byte currentChargeLevel;
        public ushort currentChargeLevelCharge;

        public byte currentMoveset;
        public sbyte currentAttackMoveset;
        public sbyte currentAttackNode;

        public ushort hitstun;
        public ushort hitstop;

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