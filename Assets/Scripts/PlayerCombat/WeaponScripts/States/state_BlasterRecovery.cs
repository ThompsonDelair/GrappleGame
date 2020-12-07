using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/UsingBlaster")]
public class state_BlasterRecovery : State {

    public float speedModifier = -0.5f;
    public override void OnStateEnter(StateDriver frame) {
        frame.actorStats.AddModifier(StatManager.StatType.Speed, speedModifier);
    }

    public override void Listen(StateDriver frame) {
        
    }

    public override void OnStateExit(StateDriver frame) {
        frame.actorStats.RemoveModifier(StatManager.StatType.Speed, speedModifier);
    }
}
