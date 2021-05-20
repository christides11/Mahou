using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    [System.Serializable]
    public abstract class FighterStat<T>
    {
        public static bool debugMode = false;

        [SerializeField] public T baseValue;
        protected T calculatedValue;
        [System.NonSerialized] protected bool isDirty = true;

        public FighterStat(T baseValue)
        {
            this.baseValue = baseValue;
        }

        public void UpdateBaseValue(T value)
        {
            baseValue = value;
            isDirty = true;
        }

        public T GetCurrentValue()
        {
            if (debugMode == true || isDirty == true)
            {
                calculatedValue = baseValue;
                isDirty = false;
            }
            return calculatedValue;
        }

        public void ForceDirty()
        {
            isDirty = true;
        }
    }
}