using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/Idle")]
public class state_PlayerIdle : State {

    public override void Listen(StateDriver frame) {

        // The fire button was pressed
        if (frame.input.ActionTriggered(InputName.Fire)) {
            frame.StateTransition(frame.weapons.chargingState);
        }

        // The grapple button was pressed
        if (frame.input.ActionTriggered(InputName.Grapple, true) && frame.weapons.gameManager.player.movement == Movement.WALKING) {
            frame.StateTransition(frame.weapons.grapplingState);
        }
    }
}
