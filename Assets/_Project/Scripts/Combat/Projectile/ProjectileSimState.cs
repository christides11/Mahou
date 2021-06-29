using Mahou.Networking;
using Mahou.Simulation;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class ProjectileSimState : ISimState
    {
        public NetworkIdentity owner;
        public Vector3 position;
        public Vector3 rotation;
        public int frameCounter;

        public override Guid GetGUID()
        {
            return StaticGetGUID();
        }

        public static new System.Guid StaticGetGUID()
        {
            return new Guid("44cc8309-d3a4-4b0d-90ea-c127d4f2b236");
        }
    }

    public class ProjectileSimStateReaderWriter : CustomISimStateReaderWriter
    {
        public override void Write(NetworkWriter writer, ISimState ss)
        {
            base.Write(writer, ss);
            ProjectileSimState pss = ss as ProjectileSimState;
            writer.WriteNetworkIdentity(pss.owner);
            writer.WriteVector3(pss.position);
            writer.WriteVector3(pss.rotation);
            writer.WriteInt(pss.frameCounter);
        }

        public override ISimState Read(NetworkReader reader)
        {
            ProjectileSimState pss = new ProjectileSimState();
            Read(reader, pss);
            return pss;
        }

        public override void Read(NetworkReader reader, ISimState ss)
        {
            base.Read(reader, ss);
            ProjectileSimState pss = ss as ProjectileSimState;
            pss.owner = reader.ReadNetworkIdentity();
            pss.position = reader.ReadVector3();
            pss.rotation = reader.ReadVector3();
            pss.frameCounter = reader.ReadInt();
        }
    }
}