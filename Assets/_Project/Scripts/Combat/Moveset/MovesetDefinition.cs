using Mahou.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Mahou.Content.Fighters
{
    [CreateAssetMenu(fileName = "MovesetDefinition", menuName = "Mahou/Combat/Moveset")]
    public class MovesetDefinition : HnSF.Combat.MovesetDefinition
    {
        public FighterStatsSO fighterStats;
        public HurtboxCollection hurtboxCollection;
        public AnimationReferenceHolder animationCollection;
    }
}