using Cysharp.Threading.Tasks;
using Mahou.Content;
using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "Battle", menuName = "Mahou/Content/Battle")]
    public class Battle : ScriptableObject
    {
        public BattleWave[] waves;
    }
}
