using Mahou.Input;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player = Rewired.Player;
using HnSF.Input;
using InputRecordItem = Mahou.Input.InputRecordItem;
using Mahou.Simulation;

namespace Mahou.Content.Fighters
{
    public class FighterInputManager : HnSF.Fighters.FighterInputManager
    {
        Player p = null;

        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
            inputRecordSize = (uint)GameConstants.gameStateBufferSize;
            InputRecord = new Mahou.Input.InputRecordItem[inputRecordSize];
        }

        public virtual void SetControllerID(int controllerID)
        {
            p = ReInput.players.GetPlayer(controllerID);
        }

        public PlayerInput SampleInputs()
        {
            PlayerInput pinput = new PlayerInput();
            if(p == null)
            {
                return pinput;
            }
            if (manager.lookHandler != null)
            {
                pinput.cameraForward = manager.lookHandler.LookTransform().transform.forward;
                pinput.cameraRight = manager.lookHandler.LookTransform().transform.right;
            }
            pinput.movement = p.GetAxis2D(Action.Movement_X, Action.Movement_Y);
            pinput.attack = p.GetButton(Action.Attack);
            pinput.jump = p.GetButton(Action.Jump);
            pinput.dash = p.GetButton(Action.Dash);
            pinput.lockon = p.GetButton(Action.Lockon);
            pinput.parry = p.GetButton(Action.Parry);
            return pinput;
        }

        public virtual void ProcessInput(int tick)
        {
            if (InputRecord[ExtDebug.mod(tick - 1, (int)inputRecordSize)] == null)
            {
                return;
            }
            
            if(InputRecord[ExtDebug.mod(tick - 1, (int)inputRecordSize)].inputs.Count == 0)
            {
                return;
            }

            foreach(var t in InputRecord[ExtDebug.mod(tick, (int)inputRecordSize)].inputs)
            {
                t.Value.Process(InputRecord[ExtDebug.mod(tick - 1, (int)inputRecordSize)].inputs[t.Key]);
            }
        }

        public void AddInput(int tick, PlayerInput pInput)
        {
            InputRecord[tick % inputRecordSize] = BuildRecordItem(pInput);
            ProcessInput((int)tick);
        }

        private InputRecordItem BuildRecordItem(PlayerInput pInput)
        {
            InputRecordItem recordItem = new InputRecordItem();
            recordItem.cameraForward = pInput.cameraForward;
            recordItem.cameraRight = pInput.cameraRight;
            recordItem.AddInput((int)PlayerInputType.MOVEMENT, new InputRecordAxis2D(pInput.movement));
            recordItem.AddInput((int)PlayerInputType.ATTACK, new InputRecordButton(pInput.attack));
            recordItem.AddInput((int)PlayerInputType.JUMP, new InputRecordButton(pInput.jump));
            recordItem.AddInput((int)PlayerInputType.DASH, new InputRecordButton(pInput.dash));
            recordItem.AddInput((int)PlayerInputType.LOCKON, new InputRecordButton(pInput.lockon));
            recordItem.AddInput((int)PlayerInputType.PARRY, new InputRecordButton(pInput.parry));
            return recordItem;
        }

        public override void ClearBuffer()
        {
            inputBufferTick = (uint)SimulationManagerBase.instance.CurrentTick;
        }

        public virtual void SetBufferTick(uint inputBufferTick)
        {
            this.inputBufferTick = inputBufferTick;
        }

        #region Buttons
        public override float GetAxis(int axis, uint frameOffset = 0)
        {
            int index = (SimulationManagerBase.instance.CurrentTick - (int)frameOffset) % (int)inputRecordSize;
            if (InputRecord[index] == null)
            {
                return 0;
            }
            return ((InputRecordAxis)InputRecord[index].inputs[axis]).axis;
        }

        public override Vector2 GetAxis2D(int axis2DID, uint frameOffset = 0)
        {
            int index = (SimulationManagerBase.instance.CurrentTick - (int)frameOffset) % (int)inputRecordSize;
            if (InputRecord[index] == null)
            {
                return Vector2.zero;
            }
            return ((InputRecordAxis2D)InputRecord[index].inputs[axis2DID]).axis2D;
        }

        public override InputRecordButton GetButton(int buttonID, uint frameOffset = 0, bool checkBuffer = false, uint bufferFrames = 3)
        {
            return GetButton(buttonID, out uint go, frameOffset, checkBuffer, bufferFrames);
        }

        public override InputRecordButton GetButton(int buttonID, out uint gotOffset, uint frameOffset = 0, bool checkBuffer = false, uint bufferFrames = 3)
        {
            int index = (SimulationManagerBase.instance.CurrentTick - (int)frameOffset) % (int)inputRecordSize;
            gotOffset = frameOffset;

            if (InputRecord[index] == null)
            {
                return new InputRecordButton();
            }
            if (checkBuffer)
            {
                for (uint i = 0; i < bufferFrames; i++)
                {
                    int bufferRealTick = SimulationManagerBase.instance.CurrentTick - (int)(frameOffset + i);
                    int bufferIndex = bufferRealTick % (int)inputRecordSize;
                    // Nothing past here.
                    if(InputRecord[bufferIndex] == null)
                    {
                        break;
                    }
                    InputRecordButton b = ((InputRecordButton)InputRecord[bufferIndex].inputs[buttonID]);
                    //Can't go further, already used buffer past here.
                    if (inputBufferTick >= bufferRealTick)
                    {
                        break;
                    }
                    if (b.firstPress)
                    {
                        gotOffset = frameOffset + i;
                        return b;
                    }
                }
            }
            return (InputRecordButton)InputRecord[index].inputs[buttonID];
        }

        public virtual Vector3 GetCameraForward(int frameOffset = 0)
        {
            int index = (SimulationManagerBase.instance.CurrentTick - (int)frameOffset) % (int)inputRecordSize;

            if (InputRecord[index] == null)
            {
                return Vector3.forward;
            }
            return (InputRecord[index] as InputRecordItem).cameraForward;
        }

        public virtual Vector3 GetCameraRight(int frameOffset = 0)
        {
            int index = (SimulationManagerBase.instance.CurrentTick - (int)frameOffset) % (int)inputRecordSize;

            if (InputRecord[index] == null)
            {
                return Vector3.right;
            }
            return (InputRecord[index] as InputRecordItem).cameraRight;
        }
        #endregion
    }
}