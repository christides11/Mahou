using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mahou.Managers;

namespace Mahou.Content.Fighters
{
    public class FighterNetwork : NetworkBehaviour
    {
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            HnSF.Fighters.LookHandler lookHandler 
                = GameObject.Instantiate(GameManager.current.GameSettings.playerCamera.gameObject, transform.position, Quaternion.identity)
                .GetComponent<HnSF.Fighters.LookHandler>();
            GetComponent<FighterManager>().lookHandler = lookHandler;
            lookHandler.SetLookAtTarget(GetComponent<FighterManager>().visual.transform);
            GetComponent<FighterInputManager>().SetControllerID(0);
        }
    }
}