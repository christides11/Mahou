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

        protected Dictionary<string, StateHurtboxDefinition> pushboxDictionary = new Dictionary<string, StateHurtboxDefinition>();
        [SerializeField] protected HurtboxEntry[] pushboxDefinitions = new HurtboxEntry[0];

        public void OnEnable()
        {
            if (hurtboxDictionary == null)
            {
                hurtboxDictionary = new Dictionary<string, StateHurtboxDefinition>();
            }
            if(pushboxDictionary == null)
            {
                pushboxDictionary = new Dictionary<string, StateHurtboxDefinition>();
            }
            hurtboxDictionary.Clear();
            pushboxDictionary.Clear();
            for (int i = 0; i < hurtboxDefinitions.Length; i++)
            {
                if (hurtboxDictionary.ContainsKey(hurtboxDefinitions[i].identifier.ToLower()))
                {
                    Debug.LogError($"{name} HurtboxCollection has a duplicate entry for {hurtboxDefinitions[i].identifier.ToLower()}.");
                    continue;
                }
                hurtboxDictionary.Add(hurtboxDefinitions[i].identifier.ToLower(), hurtboxDefinitions[i].hurtboxDefinition);
            }
            for (int i = 0; i < pushboxDefinitions.Length; i++)
            {
                if (pushboxDictionary.ContainsKey(pushboxDefinitions[i].identifier.ToLower()))
                {
                    Debug.LogError($"{name} PushboxCollection has a duplicate entry for {pushboxDefinitions[i].identifier.ToLower()}.");
                    continue;
                }
                pushboxDictionary.Add(pushboxDefinitions[i].identifier.ToLower(), pushboxDefinitions[i].hurtboxDefinition);
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

        public StateHurtboxDefinition GetPushbox(string identifier)
        {
            identifier = identifier.ToLower();
            if (pushboxDictionary.TryGetValue(identifier, out StateHurtboxDefinition pushbox))
            {
                return pushbox;
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