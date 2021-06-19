using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahou.Networking;
using Mirror;
using Mahou.Simulation;

namespace Mahou.Core
{
    public class GMGSimStateReaderWriter : PlayerSimStateReaderWriter
    {
        public override void Write(NetworkWriter writer, ISimState ss)
        {
            base.Write(writer, ss);
            GMGSimState gss = ss as GMGSimState;
            writer.Write(gss.recordMode);
            writer.Write(gss.finishedRecording);
            writer.Write(gss.currentRecordingIndex);
            //writer.WriteArray<HnSF.Input.InputRecordItem>(gss.recordBuffer);
        }

        public override ISimState Read(NetworkReader reader)
        {
            GMGSimState gss = new GMGSimState();
            Read(reader, gss);
            return gss;
        }

        public override void Read(NetworkReader reader, ISimState ss)
        {
            base.Read(reader, ss);
            GMGSimState gss = ss as GMGSimState;
            gss.recordMode = reader.ReadBool();
            gss.finishedRecording = reader.ReadBool();
            gss.currentRecordingIndex = reader.ReadInt();
            //gss.recordBuffer = reader.Read<HnSF.Input.InputRecordItem[]>();
        }
    }
}