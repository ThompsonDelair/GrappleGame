using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/Targeting")]
public class statePlayerTargeting : State {

    public override void OnStateEnter(StateDriver frame) {
        frame.weapons.FireGrapple();

    }

    public override void Listen(StateDriver frame) {
        // frame.weapons.ChargeRailCannon(false);
        frame.weapons.Grappling();
        
        // Transition back to idle when the player actor returns to it's base state
        if (frame.weapons.gameManager.player.movement == Movement.WALKING) {
            frame.weapons.Grappling();
            frame.ReturnToIdle();
        }

        // If the player inputs the Grapple Button again while grappling, Reel the grapple early and strike nearby enemies.
        if (frame.input.ActionTriggered(InputName.Grapple, true)) { // && frame.weapons.gameManager.Grappler.GrappleLanded) {
            frame.StateTransition(frame.weapons.grappleCancelState);
        }
    }

    public override void OnStateExit(StateDriver frame) {
        frame.weapons.Grappling();
    }
}
