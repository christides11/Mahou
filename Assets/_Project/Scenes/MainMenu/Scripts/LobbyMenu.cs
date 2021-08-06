using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mahou.Helpers;
using Mahou.Content;
using Mahou.Managers;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using Mahou.Networking;
using UnityEngine.UI;

namespace Mahou.Menus
{
    public class LobbyMenu : MonoBehaviour
    {
        public delegate void CloseAction(GameObject menu);
        public event CloseAction OnMenuClosed;

        public GameObject playerInfoPrefab;
        public Transform playerInfoContentHolder;
        public GameObject selectablePrefab;
        public Transform selectableContentHolder;
        public Button characterSelectButton;

        public ContentSelect contentSelect;

        public void Awake()
        {
            contentSelect.OnMenuClosed += () => { gameObject.SetActive(true); };
        }

        public async UniTask OpenMenu()
        {
            LobbyManager.current.OnLobbyInfoChanged += OnLobbySettingChanged;
            await ClearSelectables();
            gameObject.SetActive(true);
        }

        public void CloseMenu()
        {
            LobbyManager.current.OnLobbyInfoChanged -= OnLobbySettingChanged;
            gameObject.SetActive(false);
            OnMenuClosed?.Invoke(gameObject);
        }

        private void OnLobbySettingChanged(LobbyManager lobbyManager)
        {
            _ = ClearSelectables();
            UpdatePlayerList();
        }

        private void UpdatePlayerList()
        {
            foreach(Transform child in playerInfoContentHolder)
            {
                Destroy(child.gameObject);
            }

            foreach (var v in LobbyManager.current.clientLobbyInfo)
            {
                GameObject pObj = GameObject.Instantiate(playerInfoPrefab, playerInfoContentHolder, false);
            }
        }
        private async UniTask ClearSelectables()
        {
            foreach (Transform child in selectableContentHolder)
            {
                Destroy(child.gameObject);
            }

            GameObject gamemodeSelect = GameObject.Instantiate(selectablePrefab, selectableContentHolder, false);
            EventTrigger trigger = gamemodeSelect.GetComponent<EventTrigger>();
            trigger.AddOnSubmitListeners((data) => { OpenGamemodeSelect(); });

            LobbySettings lobbySettings = LobbyManager.current.Settings;
            if (String.IsNullOrEmpty(lobbySettings.selectedGamemode.objectIdentifier))
            {
                return;
            }

            await ContentManager.instance.LoadContentDefinition(ContentType.Gamemode, lobbySettings.selectedGamemode);

            IGameModeDefinition gm = (IGameModeDefinition)ContentManager.instance.GetContentDefinition(ContentType.Gamemode, lobbySettings.selectedGamemode);
            if(gm == null)
            {
                return;
            }

            TextMeshProUGUI gmSelectText = gamemodeSelect.GetComponentInChildren<TextMeshProUGUI>();
            gmSelectText.text = gm.Name;

            for(int i = 0; i < gm.ContentRequirements.Length; i++)
            {
                int currentIndex = i;

                GameObject contentSelectItem = GameObject.Instantiate(selectablePrefab, selectableContentHolder, false);
                contentSelectItem.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { OpenContentSelect(gm.ContentRequirements[currentIndex], currentIndex); });
                contentSelectItem.GetComponentInChildren<TextMeshProUGUI>().text = String.IsNullOrEmpty(lobbySettings.requiredContent[currentIndex].modIdentifier) ?
                    $"{gm.ContentRequirements[currentIndex].ToString()} Select"
                    : lobbySettings.requiredContent[currentIndex].ToString();
            }
        }

        public void OpenGamemodeSelect()
        {
            contentSelect.OnContentSelected += OnGamemodeSelected;
            contentSelect.OnMenuClosed += () => { gameObject.SetActive(true); };
            _ = contentSelect.OpenMenu(ContentType.Gamemode);
            gameObject.SetActive(false);
        }

        private void OnGamemodeSelected(ModObjectReference gamemodeReference)
        {
            contentSelect.CloseMenu();
            contentSelect.OnContentSelected -= OnGamemodeSelected;

            IGameModeDefinition gm = (IGameModeDefinition)ContentManager.instance.GetContentDefinition(ContentType.Gamemode, gamemodeReference);

            if(gm == null)
            {
                return;
            }

            List<ModObjectReference> requiredContent = new List<ModObjectReference>();
            for(int i = 0; i < gm.ContentRequirements.Length; i++)
            {
                requiredContent.Add(new ModObjectReference());
            }
            LobbyManager.current.SetLobbySettings(new LobbySettings() { selectedGamemode = gamemodeReference, requiredContent = requiredContent });
        }

        int contentIndex = 0;
        public void OpenContentSelect(ContentType contentType, int contentIndex)
        {
            this.contentIndex = contentIndex;
            contentSelect.OnContentSelected += OnContentSelected;
            _ = contentSelect.OpenMenu(contentType);
            gameObject.SetActive(false);
        }

        private void OnContentSelected(ModObjectReference contentReference)
        {
            LobbySettings lobbySettings = LobbyManager.current.Settings;
            lobbySettings.requiredContent[contentIndex] = contentReference;
            LobbyManager.current.SetLobbySettings(lobbySettings);
            contentSelect.OnContentSelected -= OnContentSelected;
            contentSelect.CloseMenu();
        }

        public void OpenCharacterSelect()
        {
            contentSelect.OnContentSelected += OnCharacterSelected;
            _ = contentSelect.OpenMenu(ContentType.Fighter);
            gameObject.SetActive(false);
        }

        private void OnCharacterSelected(ModObjectReference fighterReference)
        {
            ClientManager.local.CmdLoadFighterRequest(fighterReference);
            characterSelectButton.GetComponentInChildren<TextMeshProUGUI>().text = fighterReference.ToString();
            contentSelect.OnContentSelected -= OnCharacterSelected;
            contentSelect.CloseMenu();
        }

        public void StartMatch()
        {
            _ = LobbyManager.current.InitializeMatch();
        }
    }
}
