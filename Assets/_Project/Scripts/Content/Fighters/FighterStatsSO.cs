using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    [CreateAssetMenu(fileName = "FighterStatsSO", menuName = "Mahou/Fighter/StatsSO")]
    public class FighterStatsSO : ScriptableObject
    {
        public FighterStats baseStats;
    }
}