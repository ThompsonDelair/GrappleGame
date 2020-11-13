using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/Dodging")]
public class state_PlayerDodging : State {
    [Header("Tunable fields")]
    [SerializeField] private float dodgeRecoveryTime = 0.4f;
    [SerializeField] private float dodgeForce = 50f;


    public override void OnStateEnter(StateDriver frame) {
        frame.actorStats.AddModifier(StatManager.StatType.Speed, -1.1f);
        frame.weapons.Dodge(frame.weapons.gameManager.GetComponent<InputBuffer>().GetMovementVector(), dodgeForce);
    }

    public override void Listen(StateDriver frame) {
        State dodgeRecov = Instantiate(frame.weapons.dodgeRecoveryState);
        dodgeRecov.TimeToLive = dodgeRecoveryTime;
        frame.AddSubstate(dodgeRecov);

        // The dodge button was pressed
        if (frame.input.ActionTriggered(InputName.Dodge, true)  && frame.weapons.CanDodge && frame.weapons.gameManager.player.movement == Movement.WALKING) {
            frame.StateTransition(frame.weapons.dodgeState);
        }

        // The fire button was pressed
        if (frame.input.ActionTriggered(InputName.Fire)) {
            frame.StateTransition(frame.weapons.chargingState);
        }
    }

    public override void OnStateExit(StateDriver frame) {
        frame.actorStats.RemoveModifier(StatManager.StatType.Speed, -1.1f);
    }
}
