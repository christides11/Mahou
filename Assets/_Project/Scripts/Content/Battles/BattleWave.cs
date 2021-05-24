using Cysharp.Threading.Tasks;
using Mahou.Content;
using UnityEngine;

namespace Mahou.Content
{
    [System.Serializable]
    public class BattleWave
    {
        public BattleWaveUpdateType updateType;
        public ModObjectReference song;
        public BattleWaveEnemyGroup[] enemyGroups;
    }
}