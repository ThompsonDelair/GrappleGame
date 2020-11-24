using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDraw : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void DrawTransformsAsPolygon(Transform[] transforms, Color c) {
        for(int i = 0; i < transforms.Length; i++) {
            Debug.DrawLine(transforms[i].position,transforms[(i+1)%transforms.Length].position,c);
        }
    }

}
