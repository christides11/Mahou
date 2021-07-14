using Mahou.Simulation;
using Mirror;

namespace Mahou.Networking
{
    public class GamemodeSimStateReaderWriter
    {
        public virtual void Write(NetworkWriter writer, GameModeBaseSimState ss)
        {
            writer.WriteInt((int)ss.gameModeState);
        }

        public virtual GameModeBaseSimState Read(NetworkReader reader)
        {
            GameModeBaseSimState ss = new GameModeBaseSimState();
            Read(reader, ss);
            return ss;
        }

        public virtual void Read(NetworkReader reader, GameModeBaseSimState ss)
        {
            ss.gameModeState = (Content.GameModeState)reader.ReadInt();
        }
    }
}