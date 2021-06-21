using Mahou.Simulation;
using Mirror;
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
        }

        public void SimUpdate()
        {

            frameCounter++;
        }

        public void SimLateUpdate()
        {

        }

        public ISimState GetSimState()
        {
            return new ProjectileSimState()
            {
                networkIdentity = networkIdentity,
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
            transform.position = pss.position;
            transform.eulerAngles = pss.rotation;
            frameCounter = pss.frameCounter;
        }
    }
}