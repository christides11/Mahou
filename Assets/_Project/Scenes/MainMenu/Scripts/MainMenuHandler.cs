using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahou.Networking;
using Mirror;
using NetworkManager = Mahou.Networking.NetworkManager;
using System;

namespace Mahou.Menus
{
    public class MainMenuHandler : MonoBehaviour
    {
        public MainMenu mainMenu;
        public LobbyMenu lobbyMenu;
        public GameObject directConnectMenu;

        public void Awake()
        {
            NetworkManager.OnClientReady += OnClientReady;
            directConnectMenu.SetActive(false);
            if (NetworkClient.active)
            {
                mainMenu.OpenLobbyMenu();
            }
            else
            {
                mainMenu.gameObject.SetActive(true);
                lobbyMenu.gameObject.SetActive(false);
            }
        }

        private void OnClientReady()
        {
            mainMenu.OpenLobbyMenu();
        }
    }
}