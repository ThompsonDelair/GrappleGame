using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The TRACKER component is implemented to Update Objective Statuses.
//      Once it's objective has been completed, it sends a signal to the GameEventDirector.
//      The only aspect that must be implemented in inheriting members is the ListenForObjectiveCompletion() function.
public abstract class ObjectiveTracker : MonoBehaviour {
    protected GameManager gameManager;

    [Header("Objective Config Fields")]
    // This field can be toggled off when an objective has been completed.
    public bool objectiveActive = true;

    [SerializeField] protected List<int> objectiveIdList;
    [SerializeField] protected string currentObjective;
    [SerializeField] protected string victoryMessage;

    void Start() {
        gameManager = Object.FindObjectOfType<GameManager>();
        DisplayCurrentObjective();
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

    protected void DisplayCurrentObjective() {
        GameObject.Find("ObjectiveText").GetComponent<Text>().text = currentObjective;
    }


}
