using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FieldOfView))]
public class VitaPod : ObjectiveTracker
{
    protected FieldOfView fov;

    [Header("Vita Pod Configs")]
    public float chargeTime = 5f;
    protected float currentTime = 0f;
    public int healthCapacity = 4;
    protected int currentInterval = 1;

    [Header("Visual Configs")]
    [SerializeField] protected Light activeLight;
    [SerializeField] protected MeshRenderer fieldVisualization;
    
    private bool active = true;

    // Start is called before the first frame update
    void Start() {
        fov = this.GetComponent<FieldOfView>();
    }

    // VitaPod has been implemented as an Objective Tracker.
    // As such, it will dispatch it's event when depleted.
    protected override void ListenForObjectiveCompletion() {
        if (fov.WithinRadius(GameManager.main.player.transform.position, fov.ViewRadius) && currentTime < chargeTime) {
            currentTime += Time.deltaTime;
        }

        fov.ViewAngle = (currentTime/chargeTime) * 360 > 360 ? 360 : (currentTime/chargeTime) * 360;
        fov.DrawFieldOfView();

        // Get the intervals that allow health regen.
        float targetInterval = (chargeTime / healthCapacity) * currentInterval;

        if (currentTime >= targetInterval) {
            DamageSystem.RestoreHealth(GameManager.main.player, 1);
            currentInterval++;
        }

        if (currentTime >= chargeTime) {
            GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
            objectiveActive = false;

            if (activeLight != null) {
                activeLight.gameObject.SetActive(false);
            }

            if (fieldVisualization != null) {
                fieldVisualization.gameObject.SetActive(false);
            }
        }
    }
}
