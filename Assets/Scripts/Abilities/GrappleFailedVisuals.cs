using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// briefly shows a red line
public class GrappleFailedVisuals : MonoBehaviour
{
    float timestamp;
    public float duration;
    //public Material mat;
    GameObject grapFailedVisual;

    // Start is called before the first frame update
    void Start()
    {
        grapFailedVisual = transform.Find("GrapFailedVisual").gameObject;
        grapFailedVisual.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // turn off the visuals when timestamp expires
        if(grapFailedVisual.activeSelf && Time.time > timestamp) {
            GetComponent<LineRenderer>().positionCount = 0;
            grapFailedVisual.SetActive(false);
        }
    }

    // turn on visuals and set timestamp
    public void ShowVisual(Vector3 endPoint) {
        LineRenderer lr = GetComponent<LineRenderer>();
        Vector3[] points = { transform.position,endPoint };
        lr.SetPositions(points);
        timestamp = Time.time + duration;
        grapFailedVisual.SetActive(true);
    }
}
