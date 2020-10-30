using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDriver : MonoBehaviour
{
    public float gizmoRadius = 2f;

    // Component references

    // Used to modify the stats of the player.
    [HideInInspector] public StatManager actorStats;

    // Used to call actual functions of the weapon.
    [HideInInspector] public PlayerWeapons weapons;

    // Used to listen for player inputs.
    [HideInInspector] public InputBuffer input;

    void Start() {
        actorStats = GetComponent<StatManager>();
        weapons = GetComponent<PlayerWeapons>();
        input = GetComponent<InputBuffer>();
    }

    // To check if states should currently be listened to.
    public bool active;
    [SerializeField] protected bool canTransition;

    // Superstate starts as idle, allowing free movement. Can transition to specific attacking states.
    // Superstates are finite: Only one can be active at a time, and they must transition to some other state.
    [SerializeField] protected State restingState;
    [SerializeField] protected State finiteState;

    // Check if the current resting state is active.
    public virtual bool IsIdle() {
        return (restingState.Equals(finiteState));
    }
    
    public virtual void SetRestingState(State newState) {
        // 1. Lock transitions.
        canTransition = false;

        // 2. Cleanup all the active Substates
        for (int i = pushdownStates.Count - 1; i >= 0; i--) {
            // Make sure exit state is called
            if (!pushdownStates[i].Interruptable) {
                pushdownStates[i].OnStateExit(this);
            }
            pushdownStates.RemoveAt(i);
            pushdownExpirationTimes.RemoveAt(i);
        }

        // 3. Set the resting state
        restingState = newState;

        // 4. Set the current finite state
        if (finiteState != null) {
            finiteState.OnStateExit(this);
        }
        
        finiteState = restingState;
        finiteState.OnStateEnter(this);
        transitionTime = SetTimer(finiteState);

        // 5. Unlock transitions
        canTransition = true;
    }

    public virtual void ReturnToIdle() {
        for (int i = pushdownStates.Count - 1; i >= 0; i--) {
            // Make sure exit state is called
            if (!pushdownStates[i].Interruptable) {
                pushdownStates[i].OnStateExit(this);
            }
            pushdownStates.RemoveAt(i);
            pushdownExpirationTimes.RemoveAt(i);
        }

        StateTransition(restingState);
    }

    protected float transitionTime;

    // Pushdown States are stored in a list. Many can be active at any point.
    // Pushdown States modify the current finiteState rather than outright replacing it.
    //      A good use of pushdown states are Aiming States, Combo Recoveries, etc.
    [SerializeField] protected List<State> pushdownStates = null;
    [SerializeField] protected List<float> pushdownExpirationTimes = null;
    
    // Component States are listeners for functions that may only be performed when the StateFrame is "Idle", but are
    //      universally used regardless of what that Idle State is.
    //
    //      These are set in the inspector and are not removed at Runtime.
    [SerializeField] protected List<State> componentStates = null;

    // FixedUpdate is framerate independant
    void FixedUpdate() {
        if (!active || restingState == null) 
            return;

        // 1. listen for substate updates
        for (int i = pushdownStates.Count - 1; i >= 0; i--) {
            // Check for expiration BEFORE listening. A trigger may remove the state, rendering expiration pointless.
            if (Time.time >= pushdownExpirationTimes[i]) {
                // Debug.Log(pushdownStates[i] + " has expired.");
                RemoveSubstate(pushdownStates[i]);
            } else {
                pushdownStates[i].Listen(this);
            }
        }

        // 2. Check to see if a timed transition should occur
        if (Time.time >= transitionTime) {
            if (finiteState.TransitionState) { // If the state has it's own resting state
                StateTransition(finiteState.TransitionState);
            } else {
                StateTransition(restingState); // Otherwise, use default resting state.
            }
        }

        // 3. listen for updates.
        finiteState.Listen(this);

        // 4. If Stateframe is in an IdleState, listen to Component States
        if (IsIdle()) {
            for (int i = componentStates.Count - 1; i >= 0; i--) {
                componentStates[i].Listen(this);
            }
        }
    }

    // Replaces current Superstate with the newState, and resets the transition timer.
    // Each time a transition takes place, the states Action is called via Act().
    public virtual void StateTransition(State newState) {
        if (!canTransition) {
            return;
        }

        // If attempting to transition to the same state, and it can be refreshed...
        if (newState.Equals(finiteState) && finiteState.CanBeRefreshed) {
            // Refresh the state
            // Debug.Log("Remain in state, reset timer");
            RefreshState(newState);
        } else {
            // Transition back to resting state
            TransitionState(newState);
        }
    }

    // Reset the time to live of the given state. Does NOT call OnStateEnter, as the state is already present.
    protected void RefreshState(State newState) {
        if (newState.IsFinite) {
            transitionTime = SetTimer(newState);
        } else {
            int stateIndex = pushdownStates.FindIndex(newState.Equals);
            // Debug.Log(newState + " found at index " + stateIndex);

            pushdownExpirationTimes[stateIndex] = SetTimer(newState);
        }
    }

    // Transition from the current finite state to a new one, calling OnStateEnter
    protected void TransitionState(State newState) {
        // 1. Call exit function of current state
        finiteState.OnStateExit(this);

        // 2. Set the new current state.
        finiteState = Instantiate(newState);
        finiteState.OnStateEnter(this);
        transitionTime = SetTimer(newState);
    }

    public virtual void AddSubstate(State newState) {
        // Add the new state if it's not in the list
        if (!pushdownStates.Contains(newState)) {
            pushdownStates.Add(newState);
            pushdownExpirationTimes.Add(SetTimer(newState));
            newState.OnStateEnter(this);
        } else {
        // Refresh the state if it's found in the list.
            RefreshState(newState);
        }
    }

    public virtual void RemoveSubstate(State oldState, bool interruptState = false) {
        // Given that the state exists in the list ...
        if (pushdownStates.Contains(oldState)) {
            // Find the index of the state and it's expiration time.
            int stateIndex = pushdownStates.FindIndex(oldState.Equals);
            // Debug.Log(oldState + " found at index " + stateIndex);

            // Call it's Exit method, given the Substate is not interrupted.
            if (interruptState && oldState.Interruptable) {
                Debug.Log(oldState.name + " was interrupted. ");
            } else {
                oldState.OnStateExit(this);
            }

            // Remove both the state and it's expiration time from respective collections.
            pushdownExpirationTimes.RemoveAt(stateIndex);
            pushdownStates.RemoveAt(stateIndex);
        }
    }

    // Return a time to live of the given state
    protected float SetTimer(State state) {
        return Time.time + state.TimeToLive;
    }

    // Return the required substate from the asset menu
    // REVIEW: Perhaps Substate should implement a list of it's own State references to call when needed. Performance may suffer otherwise...?
    public virtual State GetStateFromResources(string prefix, string stateName, float stateTimeOverride = 0f) {
        string resourcePath = "_States/"+prefix+"_"+stateName;
        State newState = Resources.Load<State>(resourcePath);

        if (newState == null) {
            Debug.LogWarning("The requested state could not be found at path " + resourcePath);
            return restingState;
        }

        // If no time to live override is provided, return the existing version of the state. Otherwise, make a copy and override.
        if (stateTimeOverride > 0f) {
            State clonedState = Instantiate(newState);
            clonedState.TimeToLive = stateTimeOverride;
            return clonedState;
        } else {
            return newState;
        }
        
    }

    void OnDrawGizmos() {
        if (finiteState != null) {
            Gizmos.color = finiteState.sceneGizmoColor;
            Gizmos.DrawWireSphere(this.transform.position, gizmoRadius);
        }
    }
}
