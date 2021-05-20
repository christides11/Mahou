using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    [System.Serializable]
    public class FighterStatFloat : FighterStat<float>
    {
        public FighterStatFloat(float other) : base(other)
        {

        }

        public static implicit operator float(FighterStatFloat f) => f.GetCurrentValue();
    }
}