using Cysharp.Threading.Tasks;
using Mahou.Managers;
using Mahou.Simulation;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public class GameModeBase : MonoBehaviour
    {
        public GameModeState GameModeState { get { return gameModeState; } }
        protected GameModeState gameModeState = GameModeState.INITIALIZING;

        public virtual async UniTask<bool> SetupGamemode(ModObjectReference[] componentReferences, List<ModObjectReference> content)
        {
            for(int i = 0; i < componentReferences.Length; i++)
            {
                bool cResult = await ContentManager.instance.LoadContentDefinition(ContentType.GamemodeComponent, componentReferences[i]);
                if(cResult == false)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void Initialize()
        {
            gameModeState = GameModeState.INITIALIZING;
            LobbyManager.current.MatchManager.OnMatchStarted += OnStartMatch;
        }

        public virtual void OnStartMatch()
        {
            gameModeState = GameModeState.PRE_MATCH;
        }

        public virtual void Deconstruct()
        {
            LobbyManager.current.MatchManager.OnMatchStarted -= OnStartMatch;
        }

        public virtual void GMUpdate()
        {

        }

        public virtual void Tick()
        {

        }

        public virtual void LateTick()
        {

        }

        public virtual void SetGameModeState(GameModeState wantedState)
        {
            gameModeState = wantedState;
        }

        public virtual GameModeBaseSimState GetSimState()
        {
            return new GameModeBaseSimState()
            {
                gameModeState = gameModeState
            };
        }

        public virtual void ApplySimState(GameModeBaseSimState simState)
        {
            if(simState == null)
            {
                 SetGameModeState(GameModeState.INITIALIZING);
                return;
            }
            SetGameModeState(simState.gameModeState);
        }
    }
}