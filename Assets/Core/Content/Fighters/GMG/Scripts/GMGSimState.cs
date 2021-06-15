using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahou.Simulation;

namespace Mahou.Core
{
    public struct GMGSimState : ISimState
    {
        public PlayerSimState playerSimState;
        
        // Ability 1
        public bool recordMode;
        public bool finishedRecording;
        public int currentRecordingIndex;
        public HnSF.Input.InputRecordItem[] recordBuffer;
    }
}