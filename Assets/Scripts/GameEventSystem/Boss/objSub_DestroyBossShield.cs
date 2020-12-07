using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_DestroyBossShield : ObjectiveSubscriber {

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
            boss.invulnerable = false;
        }
        
        if (shieldObject != null) {
            shieldObject.SetActive(false);
        }

        EffectController.main.CameraShake(4f, 0.4f);
        
    }
}
