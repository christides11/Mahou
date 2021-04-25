 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mahou.Input;
using Mahou.Simulation;
using Mahou.Managers;
using Mahou.Menus;
using Mahou.Content;
using System;
using CAF.Input;
using Mahou.Content.Fighters;

namespace Mahou.Networking
{
    public class ClientManager : NetworkBehaviour
    {
        public delegate void CMAction(ClientManager cm);
        public static event CMAction OnClientManagerAdded;

        public static int playerRequestIncrement = 0;

        public static ClientManager local;

        public int InputDelay { get; protected set; } = 0;

        public NetworkIdentity networkIdentity;

        public static Dictionary<int, ClientManager> clientManagers = new Dictionary<int, ClientManager>();
        public static List<int> clientIDs = new List<int>();

        private CharacterSelectMenu characterSelect;

        public SyncList<NetworkIdentity> players = new SyncList<NetworkIdentity>();

        [SyncVar] public int clientID;

        public void Awake()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            DontDestroyOnLoad(gameObject);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            OnClientManagerAdded?.Invoke(this);
            clientManagers.Add(clientID, this);
            clientIDs.Add(clientID);
        }

        public override void OnStartAuthority()
        {
            local = this;
            GameObject g = GameObject.Instantiate(GameManager.current.GameSettings.characterSelectMenu.gameObject, transform);
            characterSelect = g.GetComponent<CharacterSelectMenu>();
            characterSelect.OpenMenu();
            characterSelect.OnCharacterSubmit += OnCharacterSelected;
        }

        [Client]
        public void SetInputDelay(int inputDelay)
        {

        }

        [Client]
        private void OnCharacterSelected(ModObjectReference fighter)
        {
            CmdRequestPlayer(fighter);
        }

        #region Spawn Player
        ModObjectReference requestFighterRef;
        List<int> unconfirmedClients = new List<int>();
        [Command]
        public async void CmdRequestPlayer(ModObjectReference fighterReference)
        {
            if(players.Count >= 1)
            {
                return;
            }

            // In the middle of loading the last request from the client.
            if (unconfirmedClients.Count > 0)
            {
                return;
            }

            if (!ModManager.instance.mods.ContainsKey(fighterReference.modIdentifier))
            {
                return;
            }
            unconfirmedClients.Clear();

            await ModManager.instance.LoadFighterDefinitions(fighterReference.modIdentifier);
            IFighterDefinition fighter = ModManager.instance.GetFighterDefinition(fighterReference);
            await fighter.LoadFighter();

            // Tell other clients to load the fighter.
            foreach (var c in NetworkServer.connections)
            {
                // Ignore host.
                if(NetworkServer.localClientActive
                    && NetworkServer.localConnection.connectionId == c.Value.connectionId)
                {
                    continue;
                }
                c.Value.Send(new LoadFighterRequestMessage()
                {
                    requestID = playerRequestIncrement,
                    fighterReference = fighterReference,
                    requestType = LoadFighterRequestMessage.RequestType.INITREQUEST
                });
                unconfirmedClients.Add(c.Value.connectionId);
            }
            lfrCurrentNumber = playerRequestIncrement;
            playerRequestIncrement++;
            requestFighterRef = fighterReference;

            if(unconfirmedClients.Count == 0)
            {
                SpawnPlayer();
                return;
            }
            ModManager.OnFighterRequestMsgResult += OnClientConfirmedLoad;
        }

        int lfrCurrentNumber;
        [Server]
        private void OnClientConfirmedLoad(NetworkConnection conn, LoadFighterRequestMessage msg)
        {
            if(msg.requestID != lfrCurrentNumber)
            {
                return;
            }
            Debug.Log($"Got confirmation. {conn.connectionId}, {msg.requestType.ToString()}");

            unconfirmedClients.Remove(conn.connectionId);

            if (msg.requestType == LoadFighterRequestMessage.RequestType.FAILED)
            {
                conn.Disconnect();
            }

            if(unconfirmedClients.Count == 0)
            {
                ModManager.OnFighterRequestMsgResult -= OnClientConfirmedLoad;
                SpawnPlayer();
            }
        }

        [Server]
        private void SpawnPlayer()
        {
            IFighterDefinition fighterDefinition = ModManager.instance.GetFighterDefinition(requestFighterRef);
            if (fighterDefinition == null)
            {
                return;
            }
            var fighterGO = fighterDefinition.GetFighter();
            if (fighterGO == null)
            {
                return;
            }
            GameObject fighter = GameObject.Instantiate(fighterGO, new Vector3(0, 1, 0), Quaternion.identity);
            NetworkServer.Spawn(fighter, gameObject);
            players.Add(fighter.GetComponent<NetworkIdentity>());
        }
        #endregion

        #region Clients
        public static List<ClientManager> GetClients()
        {
            List<ClientManager> cManagers = new List<ClientManager>();
            foreach (ClientManager cm in clientManagers.Values)
            {
                cManagers.Add(cm);
            }
            return cManagers;
        }
        #endregion

        #region Input
        public ClientInput GetInputs()
        {
            List<PlayerInput> iri = new List<PlayerInput>();

            for (int i = 0; i < players.Count; i++)
            {
                iri.Add(players[i].GetComponent<FighterInputManager>().SampleInputs());
            }
            return new ClientInput(iri);
        }

        public void AddInput(ClientInput cInputs)
        {
            if(cInputs.playerInputs == null)
            {
                return;
            }
            for(int i = 0; i < cInputs.playerInputs.Count; i++)
            {
                players[i].GetComponent<FighterInputManager>().AddInput(cInputs.playerInputs[i]);
            }
        }

        public void ReplaceInput(ClientInput cInputs, int offset)
        {
            if (cInputs.playerInputs == null)
            {
                return;
            }
            for(int i = 0; i < cInputs.playerInputs.Count; i++)
            {
                players[i].GetComponent<FighterInputManager>().ReplaceInput(offset, cInputs.playerInputs[i]);
            }
        }

        public void SetInputFrame(int offset)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<FighterInputManager>().baseOffset = offset;
            }
        }
        #endregion

        #region State
        public ClientSimState GetClientSimState()
        {
            List<PlayerSimState> pss = new List<PlayerSimState>();
            for (int i = 0; i < players.Count; i++)
            {
                pss.Add((PlayerSimState)players[i].GetComponent<ISimObject>().GetSimState());
            }

            return new ClientSimState(networkIdentity, pss);
        }

        public void ApplyClientSimState(ClientSimState clientSimState)
        {
            if(clientSimState.playersStates == null)
            {
                return;
            }
            for (int i = 0; i < clientSimState.playersStates.Count; i++)
            {
                players[i].GetComponent<ISimObject>().ApplySimState(clientSimState.playersStates[i]);
            }
        }
        #endregion

        #region Simulate Players

        public void SimulatePlayersUpdate(float dt)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<ISimObject>().SimUpdate();
            }
        }

        public void SimulatePlayersLateUpdate(float dt)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<ISimObject>().SimLateUpdate();
            }
        }
        #endregion

        #region Error Checking
        [Header("Error Checking")]
        public bool showPositions = false;
        public float positionDivergence = 0.001f;
        public bool SimComparePositions(ClientSimState serverSimState, ClientSimState localSimState, out Vector3 err)
        {
            err = Vector3.zero;
            if (localSimState.playersStates == null)
            {
                err = Vector3.zero;
                return false;
            }

            for (int i = 0; i < localSimState.playersStates.Count; i++)
            {
                Vector3 error = serverSimState.playersStates[i].motorState.Position - localSimState.playersStates[i].motorState.Position;
                if (error.sqrMagnitude > positionDivergence)
                {
                    if (showPositions)
                    {
                        ExtDebug.DrawBox(serverSimState.playersStates[i].motorState.Position + Vector3.up,
                            new Vector3(0.5f, 1, 0.5f), serverSimState.playersStates[i].motorState.Rotation, Color.blue, 1.0f);
                        ExtDebug.DrawBox(localSimState.playersStates[i].motorState.Position + Vector3.up,
                            new Vector3(0.5f, 1, 0.5f), localSimState.playersStates[i].motorState.Rotation, Color.green, 1.0f);
                    }
                    err = error;
                    return true;
                }
            }
            return false;
        }
        #endregion

        public void Interpolate(ISimState lastTick, ISimState currentTick, float alpha)
        {
            for(int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<FighterManager>().Interpolate(((ClientSimState)lastTick).playersStates[i],
                    ((ClientSimState)currentTick).playersStates[i], alpha);
            }
        }
    }
}