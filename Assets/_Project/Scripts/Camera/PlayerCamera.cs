using CAF.Fighters;
using Mahou.Content.Fighters;
using Mahou.Input;
using System.Collections;
using System.Collections.Generic;
using ThirdPersonCameraWithLockOn;
using UnityEngine;

namespace Mahou
{
    public class PlayerCamera : MonoBehaviour, CAF.Camera.LookHandler
    {
        public Camera Cam { get { return cam; } }

        [Header("References")]
        [SerializeField] private Camera cam;
        [SerializeField] private ThirdPersonCamera thirdPersonaCamera;

        [Header("Mouse")]
        [SerializeField] private float mouseDeadzone = 0.05f;
        [SerializeField] private float mouseXAxisSpeed = 1.0f;
        [SerializeField] private float mouseYAxisSpeed = 1.0f;

        [Header("Controller")]
        [SerializeField] private float stickDeadzone = 0.2f;
        [SerializeField] private float stickAxialDeadZone = 0.15f;
        [SerializeField] private float stickXAxisSpeed = 1.0f;
        [SerializeField] private float stickYAxisSpeed = 1.0f;

        private Transform followTarget;

        void Awake()
        {
            Simulation.SimulationManagerBase.OnPostUpdate += thirdPersonaCamera.ManualUpdate;
        }

        public virtual void Update()
        {
            Mahou.Input.GlobalInputManager inputManager = (Mahou.Input.GlobalInputManager)GlobalInputManager.instance;
            Vector2 stickInput = inputManager.GetAxis2D(0, Input.Action.Camera_X, Input.Action.Camera_Y);

            switch (inputManager.GetCurrentInputMethod(0))
            {
                case CurrentInputMethod.MK:
                    if (Mathf.Abs(stickInput.x) <= mouseDeadzone)
                    {
                        stickInput.x = 0;
                    }
                    if (Mathf.Abs(stickInput.y) <= mouseDeadzone)
                    {
                        stickInput.y = 0;
                    }
                    stickInput.x *= mouseXAxisSpeed;
                    stickInput.y *= mouseYAxisSpeed;
                    break;
                case CurrentInputMethod.CONTROLLER:
                    if (stickInput.magnitude < stickDeadzone)
                    {
                        stickInput = Vector2.zero;
                    }
                    else
                    {
                        float d = ((stickInput.magnitude - stickDeadzone) / (1.0f - stickDeadzone));
                        d = Mathf.Min(d, 1.0f);
                        d *= d;
                        stickInput = stickInput.normalized * d;
                    }
                    if (Mathf.Abs(stickInput.x) < stickAxialDeadZone)
                    {
                        stickInput.x = 0;
                    }
                    if (Mathf.Abs(stickInput.y) < stickAxialDeadZone)
                    {
                        stickInput.y = 0;
                    }
                    stickInput.x *= stickXAxisSpeed;
                    stickInput.y *= stickYAxisSpeed;
                    break;
            }

            thirdPersonaCamera.cameraX = stickInput.x;
            thirdPersonaCamera.cameraY = stickInput.y;
        }

        public void LookAt(Vector3 position)
        {

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
            thirdPersonaCamera.Follow = target;
            followTarget = target;
        }

        public void SetLockOnTarget(FighterBase entityTarget)
        {
            if (entityTarget == null)
            {
                thirdPersonaCamera.ExitLockOn();
                return;
            }
            thirdPersonaCamera.InitiateLockOn(entityTarget.gameObject);
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