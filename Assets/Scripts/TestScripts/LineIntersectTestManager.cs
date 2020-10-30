using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineIntersectTestManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 test = new Vector3(1,1,1);
        Debug.Log("normalized: " +test.normalized.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        Transform[] groups = Utils.GetChildren(transform);
        for(int i = 0; i < groups.Length; i++) {
            Transform[] verts = Utils.GetChildren(groups[i].transform);

            if(verts.Length >= 4) {
                if ((verts[0].position == Vector3.zero && verts[1].position == Vector3.zero) ||
                    (verts[2].position == Vector3.zero && verts[3].position == Vector3.zero)) {
                    return;
                }

                Debug.DrawLine(verts[0].position,verts[1].position,CustomColors.niceBlue);
                Debug.DrawLine(verts[2].position,verts[3].position,Color.red);
                Vector2 intersect;
                if(Calc.LineIntersect(verts[0].position,verts[1].position,verts[2].position,verts[3].position,out intersect)) {
                    Utils.debugStarPoint(intersect,0.25f,Color.yellow);
                }
            }
        }
    }

    
}
