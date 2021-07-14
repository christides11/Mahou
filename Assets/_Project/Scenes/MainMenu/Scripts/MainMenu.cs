using Mahou.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

namespace Mahou.Menus
{
    public class MainMenu : MonoBehaviour
    {

        [SerializeField] private LobbyMenu lobbyMenu;

        [Header("Direct Connect")]
        [SerializeField] private GameObject directConnectButton;
        [SerializeField] private GameObject directConnectMenu;
        [SerializeField] private TMP_InputField ipInputField;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
        public void CloseDirectConnectMenu()
        {
            directConnectMenu.SetActive(false);
        }

        public void OnButtonDirectConnectPressed()
        {
            directConnectMenu.SetActive(true);
        }

        public void OnButtonHostPressed()
        {
            NetworkManager.OnServerStartHost += OpenLobbyMenu;
            NetworkManager.singleton.StartHost();
        }

        public void OpenLobbyMenu()
        {
            _ = lobbyMenu.OpenMenu();
            directConnectMenu.SetActive(false);
            gameObject.SetActive(false);
        }

        public void OnButtonConnectPressed()
        {
            (NetworkManager.singleton as NetworkManager).JoinGame(ipInputField.text);
        }
    }
}