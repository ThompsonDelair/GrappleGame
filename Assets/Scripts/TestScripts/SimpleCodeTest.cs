using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCodeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        HashSet<Vector2> set = new HashSet<Vector2>();
        Vector2 a = new Vector2(4.4f,2.2f);
        set.Add(a);
        Vector2 b = new Vector2(4.4f,2.2f);
        if (set.Contains(b)) {
            Debug.Log("contains B");
        }
        set.Add(b);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
