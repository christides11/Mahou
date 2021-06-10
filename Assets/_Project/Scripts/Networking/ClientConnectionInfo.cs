namespace Mahou.Networking
{
    [System.Serializable]
    public class ClientConnectionInfo
    {
        public int connectionID;

        /// <summary>
        /// If the client is synced up and ready to start handling the simulation.
        /// </summary>
        public bool synced = false;

        /// <summary>
        /// Latest input tick received from this client.
        /// </summary>
        public int latestestAckedInput = 0;

        public ClientManager clientManager;
    }
}