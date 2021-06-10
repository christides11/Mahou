using Mahou.Managers;
using Mahou.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Mahou.Debugging
{
    public enum ConsoleMessageType
    {
        Debug = 0,
        Error = 1,
        Warning = 2,
        Print = 3
    }
    public class ConsoleWindow : MonoBehaviour
    {
        public static ConsoleWindow current;

        public GameObject canvasGO;

        public LobbyManager lobbyManager;

        [Header("MP Info")]
        public TextMeshProUGUI rttText;
        public TextMeshProUGUI frameText;
        public TextMeshProUGUI leadServerText;
        public TextMeshProUGUI leadLocalText;
        public TextMeshProUGUI adjText;

        [Header("Console")]
        [SerializeField] private ConsoleReader consoleReader;
        [SerializeField] private TextMeshProUGUI consoleText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private List<Color> messageColors = new List<Color>(4);

        public void Init()
        {
            current = this;
        }

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

            if (UnityEngine.Input.GetKeyDown(KeyCode.Return) && !String.IsNullOrEmpty(inputField.text))
            {
                string input = inputField.text;
                inputField.text = "";
                WriteLine($"> {input}", ConsoleMessageType.Print);
                _ = consoleReader.Convert(input);
            }

            if (lobbyManager.MatchManager != null)
            {
                UpdateMatchInfo();
            }
        }

        private void UpdateMatchInfo()
        {
            rttText.text = (Mirror.NetworkTime.rtt * 1000).ToString("F0");
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

            int currentTick = lobbyManager.MatchManager.SimulationManager.CurrentRealTick;
            frameText.text = currentTick.ToString();
            adjText.text = lobbyManager.MatchManager.SimulationManager.AdjustedInterval.ToString();
        }

        public void Write(string text)
        {
            consoleText.text += text;
        }

        public void WriteLine(string text, ConsoleMessageType msgType = ConsoleMessageType.Debug)
        {
            Write($"<#{ColorUtility.ToHtmlStringRGBA(messageColors[(int)msgType])}>" + text + "</color>");
            Write("\n");
        }

    }
}