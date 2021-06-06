using Mahou.Input;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player = Rewired.Player;
using HnSF.Input;
using InputRecordItem = Mahou.Input.InputRecordItem;

namespace Mahou.Content.Fighters
{
    public class FighterInputManager : HnSF.Fighters.FighterInputManager
    {
        Player p = null;

        public int baseOffset = 0;

        public virtual void Initialize()
        {
            manager = GetComponent<FighterManager>();
            inputRecordSize = 1024;
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
            return pinput;
        }

        public override Vector2 GetAxis2D(int axis2DID, uint frameOffset = 0)
        {
            return base.GetAxis2D(axis2DID, (uint)(baseOffset + frameOffset));
        }

        public override InputRecordButton GetButton(int buttonID, out uint gotOffset, uint frameOffset = 0, bool checkBuffer = false, uint bufferFrames = 3)
        {
            return base.GetButton(buttonID, out gotOffset, (uint)(baseOffset + frameOffset), checkBuffer, bufferFrames);
        }

        public override InputRecordButton GetButton(int buttonID, uint frameOffset = 0, bool checkBuffer = false, uint bufferFrames = 3)
        {
            return base.GetButton(buttonID, (uint)(baseOffset + frameOffset), checkBuffer, bufferFrames);
        }

        public virtual Vector3 GetCameraForward(int frameOffset = 0)
        {
            if(inputTick <= frameOffset)
            {
                return Vector3.forward;
            }
            return (InputRecord[(inputTick - 1 - frameOffset) % inputRecordSize] as InputRecordItem).cameraForward;
        }

        public virtual Vector3 GetCameraRight(int frameOffset = 0)
        {
            if (inputTick <= frameOffset)
            {
                return Vector3.right;
            }
            return (InputRecord[(inputTick - 1 - frameOffset) % inputRecordSize] as InputRecordItem).cameraRight;
        }

        public virtual void ProcessInput(int tick)
        {
            if(tick <= 1)
            {
                return;
            }

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

        public void AddInput(PlayerInput pInput)
        {
            InputRecord[inputTick % inputRecordSize] = BuildRecordItem(pInput);
            ProcessInput((int)inputTick);
            inputTick++;
        }

        public void ReplaceInput(int offset, PlayerInput pInput)
        {
            InputRecord[ExtDebug.mod(((int)inputTick)-1-offset, (int)inputRecordSize)] = BuildRecordItem(pInput);
            ProcessInput(((int)inputTick)-1-offset);
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
            return recordItem;
        }

        public override bool UsedInBuffer(int inputID, uint tick)
        {
            return false;
            //return base.UsedInBuffer(inputID, tick);
        }

        public override void ClearBuffer(int inputID)
        {
            base.ClearBuffer(inputID);
        }
    }
}