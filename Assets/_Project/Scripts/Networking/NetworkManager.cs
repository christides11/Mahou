using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mahou.Simulation;

namespace Mahou.Networking
{
    public class NetworkManager : Mirror.NetworkManager
    {
        public delegate void ServerClientAction(NetworkConnection clientConnection);
        public delegate void ServerAction(NetworkConnection clientConnection, ClientManager clientManager);
        /// <summary>
        /// Called on the server when a new client connects.
        /// </summary>
        public static event ServerClientAction OnServerClientConnected;
        /// <summary>
        /// Called on the server when a client disconnects.
        /// </summary>
        public static event ServerClientAction OnServerClientDisconnected;
        /// <summary>
        /// 
        /// </summary>
        public static event ServerAction OnServerClientReady;
        /// <summary>
        /// Called on the client when connected to a server.
        /// </summary>
        public static event ServerClientAction OnClientConnected;
        /// <summary>
        /// Called on the client when disconnected from a server.
        /// </summary>
        public static event ServerClientAction OnClientDisconnected;

        public void JoinGame(string address)
        {
            networkAddress = address;
            StartClient();
        }

        public void ClientReadyConnection()
        {
            NetworkClient.Ready();
        }

        #region Server-Side Callbacks
        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            // Ignore host.
            if (NetworkServer.localClientActive
                && conn.connectionId == NetworkServer.localConnection.connectionId)
            {
                return;
            }
            OnServerClientConnected?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            OnServerClientDisconnected?.Invoke(conn);
            base.OnServerDisconnect(conn);
        }
        #endregion

        #region Client-Side Callbacks
        public override void OnClientConnect(NetworkConnection conn)
        {
            // Host.
            if (NetworkServer.localClientActive)
            {
                base.OnClientConnect(conn);
                return;
            }
            OnClientConnected?.Invoke(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            OnClientDisconnected?.Invoke(conn);
        }
        #endregion

        public override void OnServerReady(NetworkConnection conn)
        {
            // Create the client's player.
            GameObject cManager = GameObject.Instantiate(playerPrefab);
            cManager.GetComponent<ClientManager>().clientID = conn.connectionId;
            NetworkServer.AddPlayerForConnection(conn, cManager);

            base.OnServerReady(conn);
            OnServerClientReady?.Invoke(conn, cManager.GetComponent<ClientManager>());
        }
    }
}