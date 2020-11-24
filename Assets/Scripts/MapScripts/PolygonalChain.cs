using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PolygonalChain : MonoBehaviour
{
    public bool connectEnds = true;

    //public bool hazard = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (!EditorApplication.isPlaying) {

        //}
        Transform[] children = Utils.GetChildren(transform);
        for (int i = 0; i < children.Length; i++) {

            if (i == children.Length - 1) {
                if (!connectEnds) {
                    break;
                }
            }

            Vector3 snapPosA = children[i].GetComponent<TerrainVertex>().SnapPos;
            Vector3 snapPosB = children[(i + 1) % children.Length].GetComponent<TerrainVertex>().SnapPos;

            VertType type = children[i].GetComponent<TerrainVertex>().VertType;
            Color c = Color.magenta;
            if (type == VertType.WALL) {
                c = Color.black;
            } else if (type == VertType.CLIFF) {
                c = CustomColors.darkRed;
            } else if(type == VertType.WALL_NO_GRAPPLE) {
                c = Color.red;
            }

            Debug.DrawLine(snapPosA,snapPosB,c);
        }
    }

    public void RenameChildren() {
        Transform[] children = Utils.GetChildren(transform);
        for(int i = 0; i < children.Length; i++) {
            children[i].name = i.ToString();
        }
    }


    public void CenterOnChildren() {
        Transform[] children = Utils.GetChildren(transform);
        Vector3 pos = new Vector3();
        for (int i = 0; i < children.Length; i++) {
            pos += children[i].position;
        }
        pos /= children.Length;
        Vector3 diff = pos - transform.position;
        transform.position += diff;
        for(int i = 0; i < children.Length; i++) {
            children[i].position -= diff;
        }

    }
}
