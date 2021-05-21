using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    public class GameModeBase : MonoBehaviour
    {
        protected IBattleDefinition battleDefinition;

        public virtual void InitGamemode(IBattleDefinition battleDefinition = null)
        {
            this.battleDefinition = battleDefinition;
        }
    }
}