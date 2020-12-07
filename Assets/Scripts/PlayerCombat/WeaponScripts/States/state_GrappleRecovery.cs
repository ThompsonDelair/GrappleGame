using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/GrappleRecovery")]
public class state_GrappleRecovery : State {

    public override void OnStateEnter(StateDriver frame) {
        
    }

    public override void Listen(StateDriver frame) {
        
    }

    public override void OnStateExit(StateDriver frame) {
        
        frame.weapons.ResetGrapple();
    }
}
