using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoNextMap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EndLevel());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator EndLevel() {
        yield return new WaitForSeconds(10f);
        GetComponent<SceneControl>().ShowMessageThenNextLevel("next scene \n LOL");
    }
}
