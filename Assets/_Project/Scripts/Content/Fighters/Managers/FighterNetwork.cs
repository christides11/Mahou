using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mahou.Managers;

namespace Mahou.Content.Fighters
{
    public class FighterNetwork : NetworkBehaviour
    {
        [SerializeField] private FighterManager fighterManager;
        [SerializeField] private FighterInputManager fighterInputManager;

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            CAF.Camera.LookHandler lookHandler = GameObject.Instantiate(GameManager.current.GameSettings.playerCamera.gameObject, transform.position, Quaternion.identity)
                .GetComponent<CAF.Camera.LookHandler>();
            GetComponent<FighterManager>().lookHandler = lookHandler;
            lookHandler.SetLookAtTarget(GetComponent<FighterManager>().visual.transform);
            GetComponent<FighterInputManager>().SetControllerID(0);
        }
    }
}