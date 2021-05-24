using Cysharp.Threading.Tasks;
using Mahou.Content;
using UnityEngine;

namespace Mahou.Content
{
    [System.Serializable]
    public class BattleWaveEnemyGroup
    {
        public ModObjectSharedReference[] enemies;
        [Tooltip("The current enemies that can exist before this group is spawned.")]
        public int spawnEnemyCount = 0;
    }
}