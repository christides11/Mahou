using Mahou.Combat;
using Mahou.Simulation;
using Mirror;
using System;
using System.Collections.Generic;

namespace Mahou.Networking
{
    public static class GamemodeSimStateSerializer
    {
        public static Dictionary<System.Guid, GamemodeSimStateReaderWriter> customReaderWriters = new Dictionary<System.Guid, GamemodeSimStateReaderWriter>();

        public static void Initialize()
        {
            AddReaderWriter(GameModeBaseSimState.StaticGetGUID(), new GamemodeSimStateReaderWriter());
        }

        public static void AddReaderWriter(System.Guid guid, GamemodeSimStateReaderWriter readerWriter)
        {
            if (customReaderWriters.ContainsKey(guid))
            {
                UnityEngine.Debug.Log($"Duplicate GUID of {guid} for readerwriter of type {readerWriter.GetType().FullName}");
                return;
            }
            customReaderWriters.Add(guid, readerWriter);
        }

        public static void WriteISimState(this NetworkWriter writer, GameModeBaseSimState ss)
        {
            try
            {
                writer.WriteArray<byte>(ss.GetGUID().ToByteArray());
                customReaderWriters[ss.GetGUID()].Write(writer, ss);
            }
            catch
            {
                UnityEngine.Debug.LogError($"Error writing for GUID {ss.GetGUID().ToString()}");
            }
        }

        public static GameModeBaseSimState ReadISimState(this NetworkReader reader)
        {
            System.Guid typeGuid = new Guid(reader.ReadArray<byte>());
            return customReaderWriters[typeGuid].Read(reader);
        }
    }
}