using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/GrappleCancel")]
public class state_PlayerGrappleCancel : State {
    
    public override void OnStateEnter(StateDriver frame) {
        // frame.weapons.gameManager.player
        frame.weapons.CancelGrapple();
    }

    public override void Listen(StateDriver frame) {
        // The fire button was pressed
        if (frame.input.ActionTriggered(InputName.Fire)) {
            frame.StateTransition(frame.weapons.chargingState);
        }
    }

    public override void OnStateExit(StateDriver frame) {
        
    }

}
