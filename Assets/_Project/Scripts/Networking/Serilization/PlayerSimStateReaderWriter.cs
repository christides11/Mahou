using KinematicCharacterController;
using Mahou.Simulation;
using Mirror;
using System.Collections.Generic;
using static HnSF.Combat.HitboxManager;

namespace Mahou.Networking
{
    public class PlayerSimStateReaderWriter : CustomISimStateReaderWriter
    {
        public override void Write(NetworkWriter writer, ISimState ss)
        {
            base.Write(writer, ss);
            PlayerSimState pss = ss as PlayerSimState;
            writer.Write(pss.netID);
            writer.Write(pss.visualRotation);
            writer.Write(pss.forceMovement);
            writer.Write(pss.forceGravity);
            writer.Write(pss.motorState);
            writer.Write(pss.mainState);
            writer.Write(pss.mainStateFrame);
            writer.Write(pss.currentJump);
            writer.Write(pss.isGrounded);
            writer.Write(pss.jumpHold);
            writer.Write(pss.lockedOn);
            writer.Write(pss.lockonForward);
            writer.Write(pss.lockOnTarget);
            // Input Manager
            writer.Write(pss.inputBufferTick);
            // Combat Manager
            writer.Write(pss.currentChargeLevel);
            writer.Write(pss.currentChargeLevelCharge);
            writer.Write(pss.currentMoveset);
            writer.Write(pss.currentAttackMoveset);
            writer.Write(pss.currentAttackNode);
            writer.Write(pss.hitstun);
            writer.Write(pss.hitstop);
            // Hitbox Manager
            writer.Write(pss.collidedIHurtables);
            // Hurtbox Manager
            writer.Write(pss.hurtboxHitCount);
            // State
            //writer.Write(pss.stateSimState);
        }

        public override ISimState Read(NetworkReader reader)
        {
            PlayerSimState pss = new PlayerSimState();
            Read(reader, pss);
            return pss;
        }

        public override void Read(NetworkReader reader, ISimState ss)
        {
            base.Read(reader, ss);
            PlayerSimState pss = ss as PlayerSimState;
            pss.netID = reader.ReadNetworkIdentity();
            pss.visualRotation = reader.ReadVector3();
            pss.forceMovement = reader.ReadVector3();
            pss.forceGravity = reader.ReadVector3();
            pss.motorState = reader.Read<KinematicCharacterMotorState>();
            pss.mainState = reader.ReadUShort();
            pss.mainStateFrame = reader.ReadUInt();
            pss.currentJump = reader.ReadInt();
            pss.isGrounded = reader.ReadBool();
            pss.jumpHold = reader.ReadBool();
            pss.lockedOn = reader.ReadBool();
            pss.lockonForward = reader.ReadVector3();
            pss.lockOnTarget = reader.ReadGameObject();
            // Input Manager
            pss.inputBufferTick = reader.ReadUInt();
            // Combat Manager
            pss.currentChargeLevel = reader.ReadInt();
            pss.currentChargeLevelCharge = reader.ReadInt();
            pss.currentMoveset = reader.ReadInt();
            pss.currentAttackMoveset = reader.ReadInt();
            pss.currentAttackNode = reader.ReadInt();
            pss.hitstun = reader.ReadInt();
            pss.hitstop = reader.ReadInt();
            // Hitbox Manager
            pss.collidedIHurtables = reader.Read<Dictionary<int, IDGroupCollisionInfo>>();
            // Hurtbox Manager
            pss.hurtboxHitCount = reader.Read<Dictionary<int, int>>();
            // State
            pss.stateSimState = null;
            //pss.stateSimState = reader.Read<PlayerStateSimState>();
        }
    }
}