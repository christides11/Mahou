using Mahou.Simulation;
using Mirror;

namespace Mahou.Networking
{
    public class CustomISimStateReaderWriter
    {
        public virtual void Write(NetworkWriter writer, ISimState ss)
        {
            writer.WriteNetworkIdentity(ss.networkIdentity);
            writer.WriteBool(ss.objectEnabled);
        }

        public virtual ISimState Read(NetworkReader reader)
        {
            ISimState ss = new ISimState();
            Read(reader, ss);
            return ss;
        }

        public virtual void Read(NetworkReader reader, ISimState ss)
        {
            ss.networkIdentity = reader.ReadNetworkIdentity();
            ss.objectEnabled = reader.ReadBool();
        }
    }
}