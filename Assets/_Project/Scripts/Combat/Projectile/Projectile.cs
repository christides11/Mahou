using Mahou.Simulation;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class Projectile : MonoBehaviour, ISimObject
    {
        public bool ObjectEnabled { get; protected set; } = true;

        public NetworkIdentity networkIdentity;
        public ProjectileDefinition definition;
        public NetworkIdentity owner;
        public ParticleSystem[] particleSystems;
        public GameObject[] visualGameobjects;
        public HitboxManager hitboxManager;
        public int frameCounter = 1;

        public void Initialize(NetworkIdentity owner)
        {
            this.owner = owner;
        }

        public void Enable()
        {
            ObjectEnabled = true;
        }

        public void Disable()
        {
            ObjectEnabled = false;
            for(int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Stop();
            }
            for(int j = 0; j < visualGameobjects.Length; j++)
            {
                visualGameobjects[j].SetActive(false);
            }
        }

        public void SimUpdate()
        {
            if(ObjectEnabled == false)
            {
                return;
            }

            for(int i = 0; i < definition.events.Count; i++)
            {
                HandleEvent(i, definition.events[i]);
            }
        }

        public void SimLateUpdate()
        {
            if (ObjectEnabled == false)
            {
                return;
            }

            for (int i = 0; i < definition.hitboxGroups.Count; i++)
            {
                HandleHitboxGroup(i, definition.hitboxGroups[i]);
            }

            frameCounter++;

            if(frameCounter > definition.length)
            {
                SimulationDeletionManager.RequestDeletion(networkIdentity, this);
            }
        }

        private HnSF.Combat.AttackEventReturnType HandleEvent(int i, ProjectileEventDefinition currentEvent)
        {
            if (currentEvent.active == false)
            {
                return HnSF.Combat.AttackEventReturnType.NONE;
            }

            if (frameCounter >= currentEvent.startFrame
                && frameCounter <= currentEvent.endFrame)
            {
                return currentEvent.projectileEvent.Evaluate((int)(frameCounter - currentEvent.startFrame),
                    currentEvent.endFrame - currentEvent.startFrame,
                    this);
            }
            return HnSF.Combat.AttackEventReturnType.NONE;
        }

        private void HandleHitboxGroup(int hitboxGroupIndex, HnSF.Combat.HitboxGroup hitboxGroup)
        {
            // Make sure we're in the frame window of the box.
            if (frameCounter < hitboxGroup.activeFramesStart
                || frameCounter > hitboxGroup.activeFramesEnd)
            {
                return;
            }

            // Hit check.
            switch (hitboxGroup.hitboxHitInfo.hitType)
            {
                case HnSF.Combat.HitboxType.HIT:
                    //hitboxManager.CheckForCollision(hitboxGroupIndex, hitboxGroup, gameObject, new List<GameObject>() { owner.gameObject });
                    hitboxManager.CheckForCollision(hitboxGroupIndex, hitboxGroup, owner ? owner.gameObject : null, null);
                    break;
            }
        }

        public ISimState GetSimState()
        {
            return new ProjectileSimState()
            {
                networkIdentity = networkIdentity,
                owner = owner,
                objectEnabled = ObjectEnabled,
                position = transform.position,
                rotation = transform.eulerAngles,
                frameCounter = frameCounter,
            };
        }

        public void ApplySimState(ISimState state)
        {
            ProjectileSimState pss = state as ProjectileSimState;
            ObjectEnabled = pss.objectEnabled;
            owner = pss.owner;
            transform.position = pss.position;
            transform.eulerAngles = pss.rotation;
            frameCounter = pss.frameCounter;

            switch (ObjectEnabled)
            {
                case false:
                    Disable();
                    break;
                case true:
                    Enable();
                    break;
            }
        }
    }
}