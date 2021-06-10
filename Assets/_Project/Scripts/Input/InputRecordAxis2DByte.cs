using HnSF.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Input
{
    public class InputRecordAxis2DByte : HnSF.Input.InputRecordInput
    {
        public Vector2Byte axis2D;

        public InputRecordAxis2DByte(Vector2Byte axis2D)
        {
            this.axis2D = axis2D;
        }

        public bool UsedInBuffer()
        {
            return false;
        }

        public void UseInBuffer()
        {

        }

        public void Process(InputRecordInput lastStateDown)
        {
        }
    }
}