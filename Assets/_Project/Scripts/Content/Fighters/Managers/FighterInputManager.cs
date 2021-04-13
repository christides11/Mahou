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
            pinput.movement = p.GetAxis2D(Action.Movement_X, Action.Movement_Y);
            pinput.jump = p.GetButton(Action.Jump);
            return pinput;
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

        public virtual void ProcessInput(uint tick)
        {
            if(tick <= 1)
            {
                return;
            }

            if (InputRecord[(tick - 1) % inputRecordSize] == null)
            {
                return;
            }
            
            if(InputRecord[(tick - 1) % inputRecordSize].inputs.Count == 0)
            {
                return;
            }

            foreach(var t in InputRecord[tick % inputRecordSize].inputs)
            {
                t.Value.Process(InputRecord[(tick - 1) % inputRecordSize].inputs[t.Key]);
            }
        }

        public void AddInput(PlayerInput pInput)
        {
            InputRecord[inputTick % inputRecordSize] = BuildRecordItem(pInput);
            ProcessInput(inputTick);
            inputTick++;
        }

        public void ReplaceInput(uint offset, PlayerInput pInput)
        {
            InputRecord[(inputTick-1-offset) % inputRecordSize] = BuildRecordItem(pInput);
            ProcessInput((inputTick-1-offset));
        }

        private InputRecordItem BuildRecordItem(PlayerInput pInput)
        {
            InputRecordItem recordItem = new InputRecordItem();
            recordItem.AddInput(Input.Action.Movement_X, new InputRecordAxis2D(pInput.movement));
            recordItem.AddInput(Input.Action.Jump, new InputRecordButton(pInput.jump));
            return recordItem;
        }
    }
}