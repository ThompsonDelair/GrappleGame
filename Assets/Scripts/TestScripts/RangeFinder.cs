using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeFinder : MonoBehaviour
{
    public float range;
    public Color c;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawRange() {
        Gizmos.color = c;

        Vector3 center = transform.position;
        Vector2[] circles = Calc.CircularPointsAroundPosition(Utils.Vector3ToVector2XZ(center),20,range);
        for (int i = 0; i < circles.Length; i++) {
            Debug.DrawLine(Utils.Vector2XZToVector3(circles[i]),Utils.Vector2XZToVector3(circles[(i + 1) % circles.Length]),c);
        }
        Debug.DrawLine(center,center + Vector3.forward * range,c);
        Debug.DrawLine(center,center + Vector3.back * range,c);
        Debug.DrawLine(center,center + Vector3.left * range,c);
        Debug.DrawLine(center,center + Vector3.right * range,c);
        Gizmos.DrawLine(center,center + Vector3.forward * range);

    }

    private void OnDrawGizmos() {
        DrawRange();
    }
}
