using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/GrappleCancel")]
public class state_PlayerGrappleCancel : State {
    
    [SerializeField] private float grappleRecoveryTime = 0.4f;
    public override void OnStateEnter(StateDriver frame) {
        // frame.weapons.gameManager.player
        frame.weapons.CancelGrapple();
        frame.weapons.canGrapple = false;
    }

    public override void Listen(StateDriver frame) {
        State grappleRecov = Instantiate(frame.weapons.grappleRecoveryState);
        grappleRecov.TimeToLive = grappleRecoveryTime;
        frame.AddSubstate(grappleRecov);

        // The fire button was pressed
        if (frame.input.ActionTriggered(InputName.Fire)) {
            frame.StateTransition(frame.weapons.chargingState);
        }
    }

    public override void OnStateExit(StateDriver frame) {
        
    }

}
