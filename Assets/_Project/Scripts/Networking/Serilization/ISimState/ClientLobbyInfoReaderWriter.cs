using Mirror;

namespace Mahou.Networking
{
    public static class ClientLobbyInfoReaderWriter
    {
        public static void WriteISimState(this NetworkWriter writer, ClientLobbyInfo cli)
        {
            writer.WriteNetworkBehaviour(cli.clientManager);
            writer.WriteBool(cli.initMatchSuccess);
        }

        public static ClientLobbyInfo ReadISimState(this NetworkReader reader)
        {
            ClientLobbyInfo clientLobbyInfo = new ClientLobbyInfo();
            clientLobbyInfo.clientManager = (ClientManager)reader.ReadNetworkBehaviour();
            clientLobbyInfo.initMatchSuccess = reader.ReadBool();
            return clientLobbyInfo;
        }
    }
}