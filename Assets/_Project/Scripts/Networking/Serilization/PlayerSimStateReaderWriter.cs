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
            writer.WriteFloat(pss.visualRotation);
            writer.WriteVector3(pss.forceMovementGravity);
            writer.Write(pss.motorState);
            writer.WriteUShort(pss.mainState);
            writer.WriteUInt(pss.mainStateFrame);
            writer.WriteByte(pss.currentJump);
            writer.WriteBool(pss.isGrounded);
            writer.WriteBool(pss.jumpHold);
            writer.WriteBool(pss.lockedOn);
            writer.WriteVector3(pss.lockonForward);
            writer.WriteGameObject(pss.lockOnTarget);
            // Input Manager
            writer.WriteUInt(pss.inputBufferTick);
            // Combat Manager
            writer.WriteByte(pss.currentChargeLevel);
            writer.WriteUShort(pss.currentChargeLevelCharge);
            writer.WriteByte(pss.currentMoveset);
            writer.WriteSByte(pss.currentAttackMoveset);
            writer.WriteSByte(pss.currentAttackNode);
            writer.WriteUShort(pss.hitstun);
            writer.WriteUShort(pss.hitstop);
            // Hitbox Manager
            writer.Write(pss.collidedIHurtables);
            // Hurtbox Manager
            writer.Write(pss.hurtboxHitCount);
            // State
            //writer.Write(pss.stateSimState);
            //totalByteSize = writer.Position - totalByteSize;
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
            pss.visualRotation = reader.ReadFloat();
            pss.forceMovementGravity = reader.ReadVector3();
            pss.motorState = reader.Read<KinematicCharacterMotorState>();
            pss.mainState = reader.ReadUShort();
            pss.mainStateFrame = reader.ReadUInt();
            pss.currentJump = reader.ReadByte();
            pss.isGrounded = reader.ReadBool();
            pss.jumpHold = reader.ReadBool();
            pss.lockedOn = reader.ReadBool();
            pss.lockonForward = reader.ReadVector3();
            pss.lockOnTarget = reader.ReadGameObject();
            // Input Manager
            pss.inputBufferTick = reader.ReadUInt();
            // Combat Manager
            pss.currentChargeLevel = reader.ReadByte();
            pss.currentChargeLevelCharge = reader.ReadUShort();
            pss.currentMoveset = reader.ReadByte();
            pss.currentAttackMoveset = reader.ReadSByte();
            pss.currentAttackNode = reader.ReadSByte();
            pss.hitstun = reader.ReadUShort();
            pss.hitstop = reader.ReadUShort();
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