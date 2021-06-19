using Mahou.Simulation;
using Mirror;
using System;
using System.Collections.Generic;

namespace Mahou.Networking
{
    public static class ISimStateSerializer
    {
        public static Dictionary<System.Guid, CustomISimStateReaderWriter> customReaderWriters = new Dictionary<System.Guid, CustomISimStateReaderWriter>();

        public static void Initialize()
        {
            AddReaderWriter(ISimState.StaticGetGUID(), new CustomISimStateReaderWriter());
            AddReaderWriter(PlayerSimState.StaticGetGUID(), new PlayerSimStateReaderWriter());
        }

        public static void AddReaderWriter(System.Guid guid, CustomISimStateReaderWriter readerWriter)
        {
            if (customReaderWriters.ContainsKey(guid))
            {
                return;
            }
            //UnityEngine.Debug.Log($"Adding ReaderWriter with GUID of {guid}");
            customReaderWriters.Add(guid, readerWriter);
        }

        public static void WriteISimState(this NetworkWriter writer, ISimState ss)
        {
            writer.WriteArray<byte>(ss.GetGUID().ToByteArray());
            customReaderWriters[ss.GetType().GUID].Write(writer, ss);
        }

        public static ISimState ReadISimState(this NetworkReader reader)
        {
            System.Guid typeGuid = new Guid(reader.ReadArray<byte>());
            return customReaderWriters[typeGuid].Read(reader);
        }

        public static void WritePlayerSimState(this NetworkWriter writer, PlayerSimState pss)
        {
            //UnityEngine.Debug.Log($"Writing GUID of {pss.GetGUID()}, type is {pss.GetType().FullName}");
            writer.WriteArray<byte>(pss.GetGUID().ToByteArray());
            customReaderWriters[pss.GetGUID()].Write(writer, pss);
        }

        public static PlayerSimState ReadPlayerSimState(this NetworkReader reader)
        {
            System.Guid typeGuid = new Guid(reader.ReadArray<byte>());
            //UnityEngine.Debug.Log($"Got GUID of {typeGuid}");
            return customReaderWriters[typeGuid].Read(reader) as PlayerSimState;
        }
    }
}