using KinematicCharacterController;
using Mirror;
using UnityEngine;

namespace Mahou.Simulation
{
    public struct PlayerSimState : ISimState
    {
        public NetworkIdentity netID;

        public Vector3 forceMovement;
        public Vector3 forceGravity;
        public KinematicCharacterMotorState motorState;

        public ushort mainState;
        public uint mainStateFrame;
        public int currentJump;

        public bool isGrounded;
        public bool fullHop;
    }
}