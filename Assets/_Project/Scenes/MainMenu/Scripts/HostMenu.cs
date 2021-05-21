using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mahou.Managers;
using Mahou.Content;
using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mahou.Helpers;

namespace Mahou.Menus
{
    public class HostMenu : MonoBehaviour
    {
        public delegate void CloseAction(GameObject hostMenu);
        public event CloseAction OnMenuClosed;

        List<ModObjectReference> maps;
        List<ModObjectReference> gamemodes;

        [SerializeField] private ModObjectReference selectedGamemode;
        [SerializeField] private ModObjectReference selectedMap;
        [SerializeField] private ModObjectReference selectedBattle;

        [Header("UI (General)")]
        [SerializeField] private GameObject generalTab;
        [SerializeField] private TMP_InputField lobbyName;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private UnityEngine.UI.Slider maxPlayers;
        [SerializeField] private TMP_InputField maxPing;
        [SerializeField] private TMP_InputField serverTickRate;

        [Header("UI (Gamemode)")]
        [SerializeField] private GameObject gamemodeTab;
        [SerializeField] private Transform gamemodeContentHolder;
        [SerializeField] private GameObject gamemodeContentPrefab;
        [SerializeField] private GameObject battlesUIGameObject;
        [SerializeField] private Transform battlesContentHolder;
        [SerializeField] private GameObject battleContentPrefab;

        public void CloseMenu()
        {
            maps.Clear();
            gamemodes.Clear();
            ModManager.instance.UnloadGamemodeDefinitions();
            ModManager.instance.UnloadMapDefinitions();
            OnMenuClosed?.Invoke(gameObject);
            gameObject.SetActive(false);
        }

        public async void OpenMenu()
        {
            GameManager gm = GameManager.current;
            bool mapLoadResult = await ModManager.instance.LoadMapDefinitions();
            maps = ModManager.instance.GetMapDefinitions();
            bool gamemodeLoadResult = await ModManager.instance.LoadGamemodeDefinitions();
            gamemodes = ModManager.instance.GetGamemodeDefinitions();

            OpenGeneralTab();
            gameObject.SetActive(true);
        }

        public void OpenGeneralTab()
        {
            gamemodeTab.SetActive(false);
            generalTab.SetActive(true);
            lobbyName.text = "Lobby";
            maxPlayers.value = 4;
            maxPing.text = "200";
            serverTickRate.text = "60";
        }

        public void OpenGamemodeTab()
        {
            generalTab.SetActive(false);
            gamemodeTab.SetActive(true);

            FillGamemodeList();
        }

        private void FillGamemodeList()
        {
            foreach(Transform child in gamemodeContentHolder)
            {
                Destroy(child.gameObject);
            }

            foreach (ModObjectReference mor in gamemodes)
            {
                ModObjectReference gamemodeReference = mor;
                GameObject gm = GameObject.Instantiate(gamemodeContentPrefab, gamemodeContentHolder, false);
                gm.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { OnGamemodeSelected(gamemodeReference); });
            }
        }

        public void OpenBattleSelectionMenu()
        {
            battlesUIGameObject.SetActive(true);

            foreach(Transform child in battlesContentHolder)
            {
                Destroy(child.gameObject);
            }

            _ = SetupBattleSelection();
        }

        private async UniTask SetupBattleSelection()
        {
            bool battleLoadResult = await ModManager.instance.LoadBattleDefinitions();
            var battleList = ModManager.instance.GetBattleDefinitions();

            foreach (ModObjectReference battle in battleList)
            {
                GameObject uiObject = GameObject.Instantiate(battleContentPrefab, battlesContentHolder, false);
                ModObjectReference b = new ModObjectReference(battle.modIdentifier, battle.objectIdentifier);
                uiObject.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { OnSelectedBattle(b); });
            }
        }

        private void OnSelectedBattle(ModObjectReference battle)
        {
            selectedBattle = battle;
            var bat = ModManager.instance.GetBattleDefinition(battle);
            if (bat)
            {
                selectedMap = bat.MapReference;
            }

            foreach (Transform child in battlesContentHolder)
            {
                Destroy(child.gameObject);
            }
            battlesUIGameObject.SetActive(false);
        }

        public void StartHosting()
        {
            if(selectedGamemode == null || selectedMap == null)
            {
                return;
            }

            var gamemode = ModManager.instance.GetGamemodeDefinition(selectedGamemode);

            if(gamemode.BattleSelectionRequired && selectedBattle == null)
            {
                return;
            }

            GameManager.current.LobbyManager.HostGame(new LobbySettings(selectedGamemode, selectedMap, selectedBattle));
        }

        private void OnGamemodeSelected(ModObjectReference gamemodeReference)
        {
            selectedGamemode = gamemodeReference;
        }
    }
}