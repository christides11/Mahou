using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStatsManager : MonoBehaviour
    {
        [SerializeField, Expandable] protected FighterStatsSO fighterStatsScriptableObject;
        public FighterStats baseStats;

        [Button(text: "Copy Stats From SO")]
        public virtual void CopyStatsFromScriptableObject()
        {
            baseStats = fighterStatsScriptableObject.baseStats;
        }

        [Button(text: "Copy Stats To SO")]
        public virtual void CopyStatsToScriptableObject()
        {
            fighterStatsScriptableObject.baseStats = baseStats;
        }
    }
}