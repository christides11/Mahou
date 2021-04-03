using Mahou.Input;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RewiredConsts;
using Player = Rewired.Player;
using CAF.Input;

namespace Mahou.Content.Fighters
{
    public class FighterInputManager : CAF.Fighters.FighterInputManager
    {
        Player p = null;

        public uint baseFrame = 0;

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
            pinput.movement = p.GetAxis2D(Action.Movement_X, Action.Movement_Y);
            pinput.jump = p.GetButton(Action.Jump);
            return pinput;
        }

        public void SetInput(PlayerInput input)
        {
            currentInput = input;
        }

        public override Vector2 GetAxis2D(int axis2DID, uint frameOffset = 0)
        {
            return base.GetAxis2D(axis2DID, baseFrame + frameOffset);
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

        /*
        public void AddInput(PlayerInput pInput)
        {
            InputRecordItem recordItem = new InputRecordItem();
            recordItem.AddInput(0,
                new InputRecordAxis2D(pInput.movement));
            InputRecord.Add(recordItem);
        }

        public void ReplaceInput(int offset, PlayerInput pInput)
        {
            if(InputRecord.Count <= offset)
            {
                return;
            }
            InputRecordItem recordItem = new InputRecordItem();
            recordItem.AddInput(0,
                new InputRecordAxis2D(pInput.movement));
            InputRecord[InputRecord.Count - 1 - offset] = recordItem;
        }

        public void SetBaseFrame(int offset)
        {
            baseFrame = offset;
        }*/
    }
}