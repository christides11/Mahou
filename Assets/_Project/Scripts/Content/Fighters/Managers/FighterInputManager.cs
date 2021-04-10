using Mahou.Input;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player = Rewired.Player;
using CAF.Input;

namespace Mahou.Content.Fighters
{
    public class FighterInputManager : CAF.Fighters.FighterInputManager
    {
        Player p = null;

        public uint baseOffset = 0;

        public PlayerInput currentInput;

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
                pinput.cameraEuler = manager.lookHandler.LookTransform().eulerAngles;
            }
            pinput.movement = p.GetAxis2D(Action.Movement_X, Action.Movement_Y);
            pinput.jump = p.GetButton(Action.Jump);
            return pinput;
        }

        public void SetInput(PlayerInput input)
        {
            if (manager.lookHandler != null)
            {
                manager.lookHandler.SetRotation(input.cameraEuler);
            }
            currentInput = input;
        }

        public override Vector2 GetAxis2D(int axis2DID, uint frameOffset = 0)
        {
            return base.GetAxis2D(axis2DID, baseOffset + frameOffset);
        }

        public override InputRecordButton GetButton(int buttonID, out uint gotOffset, uint frameOffset = 0, bool checkBuffer = false, uint bufferFrames = 3)
        {
            return base.GetButton(buttonID, out gotOffset, baseOffset + frameOffset, checkBuffer, bufferFrames);
        }

        public override InputRecordButton GetButton(int buttonID, uint frameOffset = 0, bool checkBuffer = false, uint bufferFrames = 3)
        {
            return base.GetButton(buttonID, baseOffset + frameOffset, checkBuffer, bufferFrames);
        }

        public void AddInput(PlayerInput pInput)
        {
            InputRecordItem recordItem = new InputRecordItem();
            recordItem.AddInput(0, new InputRecordAxis2D(pInput.movement));
            InputRecord[inputTick % inputRecordSize] = recordItem;
            inputTick++;
        }

        public void ReplaceInput(uint tick, PlayerInput pInput)
        {
            InputRecordItem recordItem = new InputRecordItem();
            recordItem.AddInput(0, new InputRecordAxis2D(pInput.movement));
            InputRecord[tick % inputRecordSize] = recordItem;
        }

        public void SetInputTick(uint tick)
        {
            inputTick = tick;
        }
    }
}