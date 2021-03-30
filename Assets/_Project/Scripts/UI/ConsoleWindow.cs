using Mahou.Managers;
using Mahou.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Mahou
{
    public class ConsoleWindow : MonoBehaviour
    {
        public GameObject canvasGO;

        public LobbyManager lobbyManager;

        public TextMeshProUGUI frameText;
        public TextMeshProUGUI leadServerText;
        public TextMeshProUGUI leadLocalText;
        public TextMeshProUGUI adjText;

        // Update is called once per frame
        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
            {
                canvasGO.SetActive(!canvasGO.activeSelf);
            }

            if (canvasGO.activeInHierarchy == false)
            {
                return;
            }

            if (lobbyManager.MatchManager != null)
            {
                UpdateMatchInfo();
            }
        }

        private void UpdateMatchInfo()
        {
            if(lobbyManager.MatchManager.SimulationManager == null)
            {
                return;
            }
            if(lobbyManager.MatchManager.SimulationManager.GetType() == typeof(ClientSimulationManager))
            {
                ClientSimulationManager csm = lobbyManager.MatchManager.SimulationManager as ClientSimulationManager;

                leadServerText.text = csm.serverTickLead.ToString();
                leadLocalText.text = csm.localTickLead.ToString();
            }

            uint currentTick = lobbyManager.MatchManager.SimulationManager.CurrentTick;
            frameText.text = (currentTick%1024).ToString();
            adjText.text = lobbyManager.MatchManager.SimulationManager.AdjustedInterval.ToString();
        }
    }
}