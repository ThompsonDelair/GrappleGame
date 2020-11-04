using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ObjectiveTracker : MonoBehaviour {
    protected GameManager gameManager;

    [Header("Config Fields")]
    public bool objectiveActive = true;

    [SerializeField] protected int objectiveId;
    [SerializeField] protected string currentObjective;
    [SerializeField] protected string victoryMessage;

    void Start() {
        gameManager = Object.FindObjectOfType<GameManager>();
        DisplayCurrentObjective();
    }

    void Update() {
        if (objectiveActive) {
            ListenForObjectiveCompletion();
        }
    }

    // Called on update if component is active. if 
    protected virtual void ListenForObjectiveCompletion() {
        
    }

    protected void DisplayCurrentObjective() {
        GameObject.Find("ObjectiveText").GetComponent<Text>().text = currentObjective;
    }


}
