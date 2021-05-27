using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HnSF.Combat.HitboxManager;

namespace Mahou
{
    public static class CustomReaderWriters
    {
        public static void WriteHitboxDictionaryType(this NetworkWriter writer, Dictionary<int, IDGroupCollisionInfo> value)
        {
            writer.WriteInt32(value.Count);
            foreach(var v in value)
            {
                writer.Write(v.Key);
                writer.Write(v.Value);
            }
        }

        public static Dictionary<int, IDGroupCollisionInfo> ReadMyType(this NetworkReader reader)
        {
            Dictionary<int, IDGroupCollisionInfo> d = new Dictionary<int, IDGroupCollisionInfo>();
            for(int i = 0; i < reader.ReadInt32(); i++)
            {
                int key = reader.Read<int>();
                IDGroupCollisionInfo value = reader.Read<IDGroupCollisionInfo>();
                d.Add(key, value);
            }
            return d;
        }
    }
}