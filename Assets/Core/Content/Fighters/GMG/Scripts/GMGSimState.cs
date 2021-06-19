using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahou.Simulation;
using System;

namespace Mahou.Core
{
    [System.Serializable]
    public class GMGSimState : PlayerSimState
    {
        // Ability 1
        public bool recordMode;
        public bool finishedRecording;
        public int currentRecordingIndex;
        public HnSF.Input.InputRecordItem[] recordBuffer;

        public override Guid GetGUID()
        {
            return StaticGetGUID();
        }

        public static new System.Guid StaticGetGUID()
        {
            return new System.Guid("23c109c4-dc94-42a2-8e51-853e91c956f4");
        }
    }
}