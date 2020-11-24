using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The TRACKER component is implemented to Update Objective Statuses.
//      Once it's objective has been completed, it sends a signal to the GameEventDirector.
//      The only aspect that must be implemented in inheriting members is the ListenForObjectiveCompletion() function.
public abstract class ObjectiveTracker : MonoBehaviour {

    [Header("Incoming ID to listen for.")]
    // This field can be toggled off when an objective has been completed.
    [SerializeField] int myDispatchID = 0;
    public bool objectiveActive = true;

    [Header("Outgoing Objective ID List.")]
    [SerializeField] protected List<int> objectiveIdList;
    // [SerializeField] protected string currentObjective;
    // [SerializeField] protected string victoryMessage;

    void Start() {
        // DisplayCurrentObjective();

        // Track for objectiveActive
        GameEventDirector.current.onObjectiveCompletion += ToggleActive;
    }


    // This 
    protected void ToggleActive(int id) {
        if (id == myDispatchID) {
            objectiveActive = !objectiveActive;
        }
    }

    // Called once per frame.
    void Update() {
        if (objectiveActive) {
            ListenForObjectiveCompletion();
        }
    }

    // Called on update if component is active.
    //      Must be overridden for inherited members.
    protected virtual void ListenForObjectiveCompletion() {
        
    }

    // protected void DisplayCurrentObjective() {
    //     GameObject.Find("ObjectiveText").GetComponent<Text>().text = currentObjective;
    // }

    private void OnDestroy() {
        GameEventDirector.current.onObjectiveCompletion -= ToggleActive;
    }


}
