using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_CaptureZone : ObjectiveTracker
{
    protected FieldOfView fov;

    [Header("Capture Zone Configs")]
    public float captureTime = 5f;
    public float currentTime = 0f;

    // Start is called before the first frame update
    void Awake() {
        fov = this.GetComponent<FieldOfView>();
    }

    protected override void ListenForObjectiveCompletion() {
        if (fov.WithinRadius(gameManager.player.transform.position, fov.ViewRadius) && currentTime < captureTime) {
            currentTime += Time.deltaTime;
        }

        fov.ViewAngle = (currentTime/captureTime) * 360 > 360 ? 360 : (currentTime/captureTime) * 360;
        fov.DrawFieldOfView();

        if (currentTime >= captureTime) {
            GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
            objectiveActive = false;
        }
    }
}
