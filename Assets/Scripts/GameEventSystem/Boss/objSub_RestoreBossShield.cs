using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_RestoreBossShield : ObjectiveSubscriber {

    [SerializeField] GameObject shieldObject;
    [SerializeField] GameObject bossObject;

    protected Actor boss;

    void Start() {
        gameManager = FindObjectOfType<GameManager>();
        GameEventDirector.current.onObjectiveCompletion += CheckObjectiveId; // This is setting what function will be called on event dispatch.
        boss = bossObject.GetComponent<ActorInfo>().actor;
    }

    protected override void OnObjectiveCompletion() {
        if (boss != null) {
            boss.invulnerable = true;
        }
        
        if (shieldObject != null) {
            shieldObject.SetActive(true);
        }
        
    }
}
