using CAF.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou
{
    public class DummyCamera : MonoBehaviour, CAF.Camera.LookHandler
    {
        public void LookAt(Vector3 position)
        {
            transform.LookAt(position);
        }

        public Transform LookTransform()
        {
            return transform;
        }

        public void Reset()
        {

        }

        public void SetLookAtTarget(Transform target)
        {
            //thirdPersonaCamera.Follow = target;
        }

        public void SetLockOnTarget(FighterBase entityTarget)
        {

        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void SetRotation(Vector3 rotation)
        {
            transform.eulerAngles = rotation;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}