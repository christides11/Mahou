using KinematicCharacterController;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static HnSF.Combat.HitboxManager;

namespace Mahou.Simulation
{
    public struct PlayerSimState : ISimState
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

        // State
        public PlayerStateSimState stateSimState;
    }
}