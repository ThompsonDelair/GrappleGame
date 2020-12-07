using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// loads the next level after ten seconds
public class AutoNextMap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EndLevel());
    }

    IEnumerator EndLevel() {
        yield return new WaitForSeconds(10f);
        GetComponent<SceneControl>().ShowMessageThenNextLevel("next scene \n LOL");
    }
}
