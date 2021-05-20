using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Mahou.Menus
{
    public class TitleScreen : MonoBehaviour
    {
        private bool transitioning = false;
        [SerializeField] private PlayableDirector pd;
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private float pdSkipTime;

        private void Awake()
        {
            pd.Play();
            pd.Pause();
        }

        private void Update()
        {
            foreach(var j in ReInput.controllers.Controllers)
            {
                if (j.GetAnyButtonDown())
                {
                    OnAnyButtonPressed();
                }
            }
        }

        public void OnAnyButtonPressed()
        {
            if (transitioning)
            {
                pd.Pause();
                pd.Play();
                pd.time = pdSkipTime;
                transitioning = false;
                mainMenu.Open();
                gameObject.SetActive(false);
                return;
            }
            transitioning = true;
            pd.Play();
        }

        public void OnQuitButtonPressed()
        {
            Application.Quit();
        }
    }
}