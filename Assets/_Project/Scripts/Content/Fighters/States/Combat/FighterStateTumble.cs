using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStateTumble : FighterState
    {
        public override string GetName()
        {
            return $"Tumble";
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnUpdate()
        {
            CheckInterrupt();
        }

        public override bool CheckInterrupt()
        {
            return false;
        }
    }
}