using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_ToggleDoor : ObjectiveSubscriber {

    [SerializeField] private int[] doorsToToggle;
    [SerializeField] private GameObject[] doorObjects;
    protected override void OnObjectiveCompletion() {

        for(int i = 0; i < doorsToToggle.Length; ++i) {
            Debug.Log("Toggling Door of ID " + doorsToToggle[i]);

            if (doorObjects[i] != null) {
                if (doorObjects[i].gameObject.activeSelf == true) {
                    doorObjects[i].gameObject.SetActive(false);
                } else {
                    doorObjects[i].gameObject.SetActive(true);
                }
                
            }

            GameManager.main.gameData.map.ToggleDoor(doorsToToggle[i]);

        }
        
    }

    // This is a stub should doors get animators with Open / Close states.
    private IEnumerator ToggleDoorAfterAnimationCompletion() {
        yield return null;
    }
}
