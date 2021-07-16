using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Combat
{
    public class Pushbox : MonoBehaviour
    {
        public BoxCollider bColl;

        public bool forceFrame = false;
        public Vector3 dir;
        public float forc;

        private void OnTriggerStay(Collider other)
        {
            forceFrame = true;

            Vector3 ourPos = transform.position;
            ourPos.y = 0;
            Vector3 theirPos = other.transform.position;
            theirPos.y = 0;
            Physics.ComputePenetration(bColl, ourPos, transform.rotation,
                other, theirPos, other.transform.rotation, out dir, out forc);
            dir.y = 0;
            dir.Normalize();
        }
    }
}