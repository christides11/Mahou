using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mahou.Combat
{
    [CustomEditor(typeof(StateHurtboxDefinition), true)]
    public class StateHurtboxDefinitionEditor : HnSF.Combat.StateHurtboxDefinitionEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}