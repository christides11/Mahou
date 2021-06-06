using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    public class FighterStatsManager : MonoBehaviour
    {
        [SerializeField] protected FighterManager fighterManager;

        public FighterStats CurrentStats { get { return currentStats; } }
        [SerializeField] private FighterStats currentStats = new FighterStats();

        public virtual void Initialize()
        {
            fighterManager = GetComponent<FighterManager>();
            FighterStat<float>.debugMode = false;
        }

        public void SetStats(FighterStatsSO statsHolder)
        {
            currentStats = new FighterStats(statsHolder.stats);
        }

        [Button(text: "Enable Editing")]
        public void DirtyStats()
        {
            FighterStat<float>.debugMode = true;
        }

        [Button(text: "Copy Stats to SO")]
        public virtual void CopyStatsToScriptableObject()
        {

        }
    }
}