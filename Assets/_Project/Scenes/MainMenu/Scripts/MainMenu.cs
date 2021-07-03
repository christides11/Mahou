using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Mahou.Menus
{
    public class MainMenu : MonoBehaviour
    {
        //[SerializeField] private PlayableDirector hostMenuTransition;
        [SerializeField] private HostMenu hostMenu;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void OnButtonHostPressed()
        {
            hostMenu.OpenMenu();
            //hostMenuTransition.Play();
            hostMenu.OnMenuClosed += (data) => { Open(); };
            gameObject.SetActive(false);
        }
    }
}