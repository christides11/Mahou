using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mahou.Networking;
using Mahou.Managers;

namespace Mahou.Menus
{
    public class IPEntryMenu : MonoBehaviour
    {
        public delegate void CloseAction(GameObject menu);
        public event CloseAction OnMenuClosed;

        public TMP_InputField ipInputField;

        public void OpenMenu()
        {
            gameObject.SetActive(true);
        }

        public void OnButtonBack()
        {
            OnMenuClosed?.Invoke(gameObject);
            gameObject.SetActive(false);
        }

        public void OnButtonConnect()
        {
            NetworkManager networkManager = GameManager.current.NetworkManager;
            networkManager.JoinGame(ipInputField.text);
        }
    }
}