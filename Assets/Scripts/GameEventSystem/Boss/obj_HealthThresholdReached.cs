using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_HealthThresholdReached : ObjectiveTracker {

    [Header("Health Threshold Tracking")]
    // Actor Fields
    [SerializeField] protected GameObject objectToTrack;
    protected ActorInfo actorInfo;

    // Phase Tracking Parameters
    [SerializeField] protected int numberOfPhases = 3;
    protected List<float> thresholds;
    protected int currentIndex;


    void Start() {
        // DisplayCurrentObjective();
        currentIndex = 0;

        // Track for objectiveActive
        GameEventDirector.current.onObjectiveCompletion += ToggleActive;

        // Check if a valid object was provided
        if (objectToTrack != null) {
            // 1. Get the actor's info
            actorInfo = objectToTrack.GetComponent<ActorInfo>();

            // 2. Add an entry to the list for each threshold.
            thresholds = new List<float>();
            for (int numberOfThresholdValues = numberOfPhases - 1; numberOfThresholdValues > 0; --numberOfThresholdValues) {
                
                thresholds.Add( (actorInfo.stats.maxHP / numberOfPhases) * numberOfThresholdValues );
            }

        } else {
            Debug.LogWarning("No health object provided to " + this.name);
            objectiveActive = false;
        }
    }

    protected override void ListenForObjectiveCompletion() {

        if (thresholds.Count > 0) {
            // Check if the current health value is below the current threshold. If it is, dispatch ID.

            if (actorInfo.actor.health <= thresholds[currentIndex]) {
                // TODO Fix this. Straight up a hack. No time to fix, Should be it's own component.
                if (currentIndex == 0) {
                    List<int> firstPhaseCompletion = new List<int>();
                    firstPhaseCompletion.Add(4);
                    GameEventDirector.current.ObjectiveCompletion(firstPhaseCompletion);
                }

                GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
                currentIndex++;
            }

            // Check if the final threshold has been reached. If so, disable tracking.
            if (currentIndex == thresholds.Count) {
                objectiveActive = false;
            }

        }
        
    }
}
