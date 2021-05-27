using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    [CreateAssetMenu(fileName = "HurtboxCollection", menuName = "Mahou/Content/HurtboxCollection")]
    public class HurtboxCollection : ScriptableObject
    {
        [System.Serializable]
        public class HurtboxEntry
        {
            public string identifier;
            public StateHurtboxDefinition hurtboxDefinition;
        }

        protected Dictionary<string, StateHurtboxDefinition> hurtboxDictionary = new Dictionary<string, StateHurtboxDefinition>();
        [SerializeField] protected HurtboxEntry[] hurtboxDefinitions = new HurtboxEntry[0];

        public void OnEnable()
        {
            if (hurtboxDictionary == null)
            {
                hurtboxDictionary = new Dictionary<string, StateHurtboxDefinition>();
            }
            hurtboxDictionary.Clear();
            for (int i = 0; i < hurtboxDefinitions.Length; i++)
            {
                if (hurtboxDictionary.ContainsKey(hurtboxDefinitions[i].identifier.ToLower()))
                {
                    Debug.LogError($"{name} HurtboxCollection has a duplicate entry for {hurtboxDefinitions[i].identifier.ToLower()}.");
                    continue;
                }
                hurtboxDictionary.Add(hurtboxDefinitions[i].identifier.ToLower(), hurtboxDefinitions[i].hurtboxDefinition);
            }
        }

        public StateHurtboxDefinition GetHurtbox(string identifier)
        {
            identifier = identifier.ToLower();
            if (hurtboxDictionary.TryGetValue(identifier, out StateHurtboxDefinition hurtbox))
            {
                return hurtbox;
            }
            return null;
        }

        public bool TryGetAnimation(string identifier, out StateHurtboxDefinition hurtbox)
        {
            hurtbox = null;
            if (hurtboxDictionary.TryGetValue(identifier, out hurtbox))
            {
                return true;
            }
            return false;
        }
    }
}