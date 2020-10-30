using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ObjectiveListener : MonoBehaviour {

    protected SceneControl sceneController;
    protected GameManager gameManager;

    [Header("Config Fields")]
    public bool objectiveActive = true;

    [SerializeField] protected string currentObjective;
    [SerializeField] protected string victoryMessage;

    void Start() {
        gameManager = Object.FindObjectOfType<GameManager>();
        sceneController = gameManager.GetComponent<SceneControl>();

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
