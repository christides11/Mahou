using HnSF.Combat;
using Mahou.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    public class HitboxBlinker : MonoBehaviour, ISimObject
    {
        public HitboxManager hitboxManager;
        public int hitboxRepeatFrame = 10;
        public HitboxGroup hitboxGroup;
        public GameObject visual;

        public int hitstop = 0;

        private void OnValidate()
        {
            if((hitboxGroup.hitboxHitInfo as Mahou.Combat.HitInfo) == null)
            {
                hitboxGroup.hitboxHitInfo = new Mahou.Combat.HitInfo();
            }
        }

        void Awake()
        {
            hitboxGroup.boxes.Add(new Mahou.Combat.BoxDefinition());
            (hitboxGroup.boxes[0] as Mahou.Combat.BoxDefinition).shape = BoxShape.Rectangle;
            (hitboxGroup.boxes[0] as Mahou.Combat.BoxDefinition).size = new Vector3(5, 5, 5);
            (hitboxGroup.boxes[0] as Mahou.Combat.BoxDefinition).offset = new Vector3(0, 0, 0);
            hitboxManager.OnHitHurtbox += onHitHurtbox;
        }

        private void onHitHurtbox(HitboxGroup hitboxGroup, int hitboxIndex, HnSF.Combat.Hurtbox hurtbox)
        {
            hitstop = (hitboxGroup.hitboxHitInfo as Mahou.Combat.HitInfo).attackerHitstop;
        }

        public void SimUpdate()
        {

        }

        public void SimLateUpdate()
        {
            if(hitstop > 0)
            {
                hitstop--;
                return;
            }
            visual.SetActive(false);
            if(SimulationManagerBase.instance.CurrentTick % hitboxRepeatFrame == 0)
            {
                visual.SetActive(true);
                if(hitboxManager.CheckForCollision(0, hitboxGroup))
                {

                }
            }
            hitboxManager.Reset();
        }

        public void ApplySimState(ISimState state)
        {

        }

        public ISimState GetSimState()
        {
            return null;
        }
    }
}