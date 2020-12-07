using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/Firing")]
public class state_PlayerFiring : State {

    public override void Listen(StateDriver frame) {
        frame.AddSubstate(frame.weapons.blasterRecoveryState);

        // Listen for grapple inputs directly after firing.
        if (frame.input.ActionTriggered(InputName.Grapple, true) && frame.weapons.canGrapple && frame.weapons.gameManager.player.currMovement == Movement.WALKING) {
            frame.StateTransition(frame.weapons.grapplingState);
        }

    }

}
