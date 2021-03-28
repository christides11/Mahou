using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject playButtonOptions;
        [SerializeField] private HostMenu hostMenu;
        [SerializeField] private IPEntryMenu ipEntryMenu;

        public void OnButtonPlayPressed()
        {
            playButtonOptions.SetActive(!playButtonOptions.activeSelf);
        }

        public void OnButtonHostPressed()
        {
            hostMenu.OpenMenu();
            hostMenu.OnMenuClosed += (data) => { gameObject.SetActive(true); };
            gameObject.SetActive(false);
        }

        public void OnButtonJoinPressed()
        {
            ipEntryMenu.OpenMenu();
            //ipEntryMenu.OnMenuClosed += (data) => {  };
        }
    }
}