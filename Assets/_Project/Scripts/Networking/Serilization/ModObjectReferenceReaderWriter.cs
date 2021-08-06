using Mahou.Content;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Networking
{
    public static class ModObjectReferenceReaderWriter
    {
        public static void WriteISimState(this NetworkWriter writer, ModObjectReference ss)
        {
            writer.WriteString(ss.modIdentifier);
            writer.WriteString(ss.objectIdentifier);
        }

        public static ModObjectReference ReadISimState(this NetworkReader reader)
        {
            return new ModObjectReference(reader.ReadString(), reader.ReadString());
        }
    }
}