using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this component is attached to gameobjects that will be linked to actors
// the actorstats is grabbed when the actor class is created
// the game object's actor can also be referenced from here
public class ActorInfo : MonoBehaviour
{
    public ActorStats stats;
    public Actor actor;

}
