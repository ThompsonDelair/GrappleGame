using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorUtil
{
    public static bool RaycastMousePosZeroYEditor(out Vector3 worldPos) {

        // We create an invisible plane at world pos 0,0,0
        Plane groundPlane = new Plane(Vector3.up,Vector3.zero);
        float rayLength;

        // Here's the ray. We cast from the camera to the mouse position.
        Ray cameraRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        //Ray cameraRay;
        //if(Camera.main != null) {
        //    cameraRay = sceneView.camera.ScreenPointToRay(Input.mousePosition);
        //} else {
        //    worldPos = Vector3.negativeInfinity;
        //    return false;
        //}
        

        // If we intersect with the ground plane, we can get it's point of intersection and return that.
        if (groundPlane.Raycast(cameraRay,out rayLength)) {
            worldPos = cameraRay.GetPoint(rayLength);
            return true;
        }

        worldPos = Vector3.negativeInfinity;
        return false;

    }
}
