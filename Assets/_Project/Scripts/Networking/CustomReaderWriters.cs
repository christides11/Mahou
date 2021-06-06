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

        public static Dictionary<int, IDGroupCollisionInfo> ReadHitboxDictionaryType(this NetworkReader reader)
        {
            Dictionary<int, IDGroupCollisionInfo> d = new Dictionary<int, IDGroupCollisionInfo>();
            int valueCount = reader.ReadInt32();
            for(int i = 0; i < valueCount; i++)
            {
                int key = reader.ReadInt32();
                IDGroupCollisionInfo value = reader.Read<IDGroupCollisionInfo>();
                d.Add(key, value);
            }
            return d;
        }

        public static void WriteIntBoolDictionaryType(this NetworkWriter writer, Dictionary<int, bool> value)
        {
            writer.WriteInt32(value.Count);
            foreach(var v in value)
            {
                writer.WriteInt32(v.Key);
                writer.WriteBoolean(v.Value);
            }
        }

        public static Dictionary<int, bool> ReadIntBoolDictionary(this NetworkReader reader)
        {
            Dictionary<int, bool> d = new Dictionary<int, bool>();
            int valueCount = reader.ReadInt32();
            for (int i = 0; i < valueCount; i++)
            {
                d.Add(reader.ReadInt32(), reader.ReadBoolean());
            }
            return d;
        }

        public static void WriteIDGroupCollisionInfoType(this NetworkWriter writer, IDGroupCollisionInfo value)
        {
            writer.WriteInt32(value.hitIHurtables.Count);
            for(int i = 0; i < value.hitIHurtables.Count; i++)
            {
                writer.WriteGameObject(value.hitIHurtables[i]);
            }
            writer.WriteInt32(value.hitboxGroups.Count);
            foreach(int v in value.hitboxGroups)
            {
                writer.WriteInt32(v);
            }
        }

        public static IDGroupCollisionInfo ReadIDGroupCollisionInfoType(this NetworkReader reader)
        {
            IDGroupCollisionInfo d = new IDGroupCollisionInfo();
            int valueCount = reader.ReadInt32();
            for(int i = 0; i < valueCount; i++)
            {
                d.hitIHurtables.Add(reader.ReadGameObject());
            }
            int hitboxGroupValueCount = reader.ReadInt32();
            for(int i = 0; i < hitboxGroupValueCount; i++)
            {
                d.hitboxGroups.Add(reader.ReadInt32());
            }
            return d;
        }
    }
}