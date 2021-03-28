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

namespace Mahou.Menus
{
    public class HostMenu : MonoBehaviour
    {
        public delegate void CloseAction(GameObject hostMenu);
        public event CloseAction OnMenuClosed;

        public TMP_Dropdown gamemodeDropdown;
        public TMP_Dropdown mapDropdown;

        List<ModObjectReference> maps;
        List<ModObjectReference> gamemodes;

        [SerializeField] private ModObjectReference selectedGamemode;
        [SerializeField] private ModObjectReference selectedMap;

        public void CloseMenu()
        {
            maps.Clear();
            gamemodes.Clear();
            ModManager.instance.UnloadGamemodeDefinitions();
            ModManager.instance.UnloadMapDefinitions();
            OnMenuClosed?.Invoke(gameObject);
            gameObject.SetActive(false);
            gamemodeDropdown.onValueChanged.RemoveAllListeners();
            mapDropdown.onValueChanged.RemoveAllListeners();
        }

        public async void OpenMenu()
        {
            GameManager gm = GameManager.current;
            bool mapLoadResult = await ModManager.instance.LoadMapDefinitions();
            maps = ModManager.instance.GetMapDefinitions();
            bool gamemodeLoadResult = await ModManager.instance.LoadGamemodeDefinitions();
            gamemodes = ModManager.instance.GetGamemodeDefinitions();
            FillGamemodeDropdown();
            FillMapDropdown();

            gamemodeDropdown.onValueChanged.AddListener((data) => { OnGamemodeSelected(data); });
            mapDropdown.onValueChanged.AddListener((data) => { OnMapSelected(data); });
            OnGamemodeSelected(1);
            OnMapSelected(1);
            gameObject.SetActive(true);
        }

        public void StartHosting()
        {
            if(selectedGamemode == null
                || selectedMap == null)
            {
                return;
            }

            GameManager.current.LobbyManager.HostGame(new LobbySettings(selectedGamemode, selectedMap));
        }

        private void OnMapSelected(int value)
        {
            if(value == 0)
            {
                selectedMap = null;
                return;
            }
            if(maps.Count < value)
            {
                return;
            }
            selectedMap = maps[value - 1];
        }

        private void OnGamemodeSelected(int value)
        {
            if(value == 0)
            {
                selectedGamemode = null;
                return;
            }
            if(gamemodes.Count < value)
            {
                return;
            }
            selectedGamemode = gamemodes[value - 1];
        }

        private void FillMapDropdown()
        {
            mapDropdown.ClearOptions();
            mapDropdown.AddOptions(new List<string> { "N/A" });

            List<string> options = new List<string>();
            foreach(ModObjectReference mor in maps)
            {
                IMapDefinition mapDefinition = ModManager.instance.GetMapDefinition(mor);
                options.Add(mapDefinition.Name);
            }
            mapDropdown.AddOptions(options);
        }

        private void FillGamemodeDropdown()
        {
            gamemodeDropdown.ClearOptions();
            gamemodeDropdown.AddOptions(new List<string> { "N/A" });

            List<string> options = new List<string>();
            foreach (ModObjectReference mor in gamemodes)
            {
                IGameModeDefinition gmd = ModManager.instance.GetGamemodeDefinition(mor);
                options.Add(gmd.Name);
            }
            gamemodeDropdown.AddOptions(options);
        }
    }
}