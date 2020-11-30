using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(menuName = "CombatStates/Charging")]
public class state_PlayerCharging : State
{

    [Header("Tunable fields")]
    [SerializeField] private float lightFiringTime = 0.1f;
    [SerializeField] private float heavyFiringTime = 1.2f;

    public override void OnStateEnter(StateDriver frame) {
        frame.weapons.EnterChargeState();
        SoundManager.StartClipOnActor(AudioClips.singleton.chargeShotCharge, GameManager.main.player, 6f, false);
    }

    public override void Listen(StateDriver frame) {
        frame.AddSubstate(frame.weapons.blasterRecoveryState);

        // Increase Charge Time
        frame.weapons.ChargeRailCannon();

        // Once button is released, check charge threshold and transition to firing.
        if (!frame.input.ActionHeld(InputName.Fire)) {

            if (frame.weapons.ChargeThresholdReached) {
                Debug.Log("Charging threshold reached");
                frame.weapons.FireRailCannon();

                State heavyFiringState = Instantiate(frame.weapons.firingState);
                heavyFiringState.TimeToLive = heavyFiringTime;
                frame.StateTransition(heavyFiringState);

            } else {
                
                frame.weapons.FireBlaster();

                State lightFiringState = Instantiate(frame.weapons.firingState);
                lightFiringState.TimeToLive = lightFiringTime;
                frame.StateTransition(lightFiringState);
            }

        // Case that the charge time is about to time-out the state.
        } else if (frame.weapons.chargeTime >= timeToLive * 0.95f) {
            Debug.Log("Charging timed out");

            frame.weapons.FireRailCannon();

            State heavyFiringState = Instantiate(frame.weapons.firingState);
            heavyFiringState.TimeToLive = heavyFiringTime;
            frame.StateTransition(heavyFiringState);
        }

    }

    // Added for stopping the sound started during state
    public override void OnStateExit(StateDriver frame)
    {
        SoundManager.StopClipOnActor(AudioClips.singleton.chargeShotCharge, GameManager.main.player);
    }

}
