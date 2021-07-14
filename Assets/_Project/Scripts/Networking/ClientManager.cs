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
using HnSF.Input;
using Mahou.Content.Fighters;

namespace Mahou.Networking
{
    public class ClientManager : NetworkBehaviour
    {
        public delegate void CMAction(ClientManager cm);
        public static event CMAction OnClientManagerAdded;

        public static int playerRequestIncrement = 0;

        public static ClientManager local;

        public NetworkIdentity networkIdentity;

        public static Dictionary<int, ClientManager> clientManagers = new Dictionary<int, ClientManager>();
        public static List<int> clientIDs = new List<int>();

        //private CharacterSelectMenu characterSelect;

        public List<ModObjectReference> fighters = new List<ModObjectReference>();

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

        public override void OnStartServer()
        {
            base.OnStartServer();
        }

        public override void OnStartAuthority()
        {
            local = this;
        }

        #region Spawn Player
        [Command]
        public async void CmdLoadFighterRequest(ModObjectReference fighterReference)
        {
            bool loadResult = await NetworkFighterSpawnManager.ServerRequestFighterLoad(fighterReference);

            if(loadResult == false)
            {
                Debug.Log($"SERVER: Failed loading {fighterReference} for {networkIdentity.connectionToClient.connectionId}.");
                return;
            }

            fighters[0] = fighterReference;
        }

        [Server]
        public GameObject SpawnPlayerFighter(ModObjectReference requestFighterRef, Vector3 position, Quaternion rotation)
        {
            if(requestFighterRef == null)
            {
                return null;
            }
            IFighterDefinition fighterDefinition = (IFighterDefinition)ContentManager.instance.GetContentDefinition(ContentType.Fighter, requestFighterRef);
            if (fighterDefinition == null)
            {
                return null;
            }
            var fighterGO = fighterDefinition.GetFighter();
            if (fighterGO == null)
            {
                return null;
            }
            GameObject fighter = GameObject.Instantiate(fighterGO, position, rotation);
            NetworkServer.Spawn(fighter, new System.Guid(fighterDefinition.GetFighterGUID()), networkIdentity.connectionToClient);
            players.Add(fighter.GetComponent<NetworkIdentity>());
            return fighter;
        }
        #endregion

        #region Clients
        public static ClientManager GetClient(int clientID)
        {
            if (clientManagers.TryGetValue(clientID, out ClientManager cm))
            {
                return cm;
            }
            return null;
        }

        public static ClientManager GetClient(NetworkIdentity networkIdentity)
        {
            foreach (ClientManager cm in clientManagers.Values)
            {
                if(cm.networkIdentity.netId == networkIdentity.netId)
                {
                    return cm;
                }
            }
            return null;
        }

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

        public void SetInput(int tick, ClientInput clientInput)
        {
            if(clientInput.playerInputs == null)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].GetComponent<FighterInputManager>().AddInput(tick, new PlayerInput());
                }
                return;
            }
            for (int i = 0; i < clientInput.playerInputs.Count; i++)
            {
                players[i].GetComponent<FighterInputManager>().AddInput(tick, clientInput.playerInputs[i]);
            }
        }
        #endregion

        #region State
        public ClientSimState GetClientSimState()
        {
            List<PlayerSimState> pss = new List<PlayerSimState>();
            for (int i = 0; i < players.Count; i++)
            {
                pss.Add(players[i].GetComponent<ISimObject>().GetSimState() as PlayerSimState);
            }

            return new ClientSimState(networkIdentity, pss);
        }

        public void ApplyClientSimState(ClientSimState clientSimState)
        {
            if(clientSimState == null || clientSimState.playersStates == null)
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
        public float rotationDivergence = 0.001f;
        public bool CompareSimulationStates(ClientSimState serverSimState, ClientSimState localSimState)
        {
            if (localSimState == null || localSimState.playersStates == null)
            {
                return false;
            }

            for (int i = 0; i < localSimState.playersStates.Count; i++)
            {
                Vector3 posError = serverSimState.playersStates[i].motorState.Position - localSimState.playersStates[i].motorState.Position;
                if (posError.sqrMagnitude > positionDivergence)
                {
                    if (showPositions)
                    {
                        ExtDebug.DrawBox(serverSimState.playersStates[i].motorState.Position + Vector3.up,
                            new Vector3(0.5f, 1, 0.5f), serverSimState.playersStates[i].motorState.Rotation, Color.blue, 1.0f);
                        ExtDebug.DrawBox(localSimState.playersStates[i].motorState.Position + Vector3.up,
                            new Vector3(0.5f, 1, 0.5f), localSimState.playersStates[i].motorState.Rotation, Color.green, 1.0f);
                    }
                    return true;
                }
                float rotError = Mathf.Abs(serverSimState.playersStates[i].visualRotation - localSimState.playersStates[i].visualRotation);
                if(rotError > rotationDivergence)
                {
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
                if(((ClientSimState)lastTick).playersStates == null
                    || ((ClientSimState)lastTick).playersStates.Count < players.Count)
                {
                    return;
                }
                players[i].GetComponent<FighterManager>().Interpolate(
                    (PlayerSimState)((ClientSimState)lastTick).playersStates[i],
                    (PlayerSimState)((ClientSimState)currentTick).playersStates[i], alpha);
            }
        }
    }
}