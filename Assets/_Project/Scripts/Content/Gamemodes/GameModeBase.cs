using Cysharp.Threading.Tasks;
using Mahou.Simulation;
using UnityEngine;

namespace Mahou.Content
{
    public class GameModeBase : MonoBehaviour
    {
        public GameModeState GameModeState { get { return gameModeState; } }

        protected IBattleDefinition battleDefinition;

        protected GameModeState gameModeState = GameModeState.INITIALIZING;

        /// <summary>
        /// Load anything here that the gamemode will need.
        /// </summary>
        /// <returns>True if everything loaded successfully.</returns>
        public virtual async UniTask<bool> LoadRequirements()
        {
            return true;
        }

        public virtual void Initialize(IBattleDefinition battleDefinition = null)
        {
            gameModeState = GameModeState.INITIALIZING;
            this.battleDefinition = battleDefinition;
        }

        public virtual void Update()
        {

        }

        public virtual void Tick()
        {

        }

        public virtual void SetGameModeState(GameModeState wantedState)
        {

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