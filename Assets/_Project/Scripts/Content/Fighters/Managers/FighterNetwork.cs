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

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (hasAuthority)
            {
                return;
            }
            fighterManager.lookHandler = GameObject.Instantiate(GameManager.current.GameSettings.dummyCamera.gameObject, transform.position, Quaternion.identity)
                .GetComponent<CAF.Camera.LookHandler>();
            fighterManager.lookHandler.SetLookAtTarget(transform);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Local player.
            if (hasAuthority)
            {
                fighterManager.lookHandler = GameObject.Instantiate(GameManager.current.GameSettings.playerCamera.gameObject, transform.position, Quaternion.identity)
                    .GetComponent<CAF.Camera.LookHandler>();
            }
            else
            {
                fighterManager.lookHandler = GameObject.Instantiate(GameManager.current.GameSettings.dummyCamera.gameObject, transform.position, Quaternion.identity)
                    .GetComponent<CAF.Camera.LookHandler>();
            }
            fighterManager.lookHandler.SetLookAtTarget(transform);
        }
    }
}