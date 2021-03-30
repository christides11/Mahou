using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CAF.Fighters;
using Mahou.Simulation;
using Mirror;
using KinematicCharacterController;

namespace Mahou.Content.Fighters
{
    public class FighterManager : FighterBase, ISimObject
    {
        public NetworkIdentity netid;
        public FighterCharacterController cc;
        public float movSpeed = 0.5f;

        public virtual void Awake()
        {
            SetupStates();
            KinematicCharacterSystem.Settings.AutoSimulation = false;
            KinematicCharacterSystem.Settings.Interpolate = false;
        }

        public virtual void SetupStates()
        {

        }

        public void SimUpdate()
        {
            Vector2 mov = (InputManager as FighterInputManager).currentInput.movement;
            cc.Motor.SetPosition(cc.Motor.InitialSimulationPosition + (new Vector3(mov.x, 0, mov.y) * 0.3f));
            //Tick();
        }

        public void SimLateUpdate()
        {
            //LateTick();
        }

        public ISimState GetSimState()
        {
            PlayerSimState simState = new PlayerSimState();
            simState.netID = netid;
            simState.motorState = cc.Motor.GetState();
            simState.forceMovement = (physicsManager as FighterPhysicsManager3D).forceMovement;
            simState.forceGravity = (physicsManager as FighterPhysicsManager3D).forceGravity;
            simState.mainState = (StateManager as FighterStateManager).CurrentState;
            simState.mainStateFrame = (StateManager as FighterStateManager).CurrentStateFrame;
            return simState;
        }

        public void ApplySimState(ISimState state)
        {
            PlayerSimState pState = (PlayerSimState)state;
            cc.Motor.ApplyState(pState.motorState);
            (physicsManager as FighterPhysicsManager3D).forceMovement = pState.forceMovement;
            (physicsManager as FighterPhysicsManager3D).forceGravity = pState.forceGravity;
            (StateManager as FighterStateManager).ChangeState(pState.mainState, pState.mainStateFrame);
        }
    }
}