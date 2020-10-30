using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Objective : ScriptableObject {
    public string objectiveMessage;
    public string successMessage;

    // Will be checked by an objective listener, which will check the current objective.
    public virtual bool ObjectiveComplete() {
        return false;
    }

}
