using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a script that maintains the camera on a player Transform.
//      It's useful given that the player may be rapdily rotating in-game, 
//      and we don't want the camera to inherit that rotation.
public class CameraController : MonoBehaviour {
    private Vector3 cameraTarget;
    private Camera playerCamera = null;
    private Transform target = null;

    [SerializeField] protected float followSpeed = 0.05f;
    [SerializeField] protected float locOffsetX = 0;
    [SerializeField] protected float locOffsetY = 32f;
    [SerializeField] protected float locOffsetZ = -9f;

    void Start() {
        playerCamera = GetComponentInChildren<Camera>();
        playerCamera.transform.localRotation = Quaternion.Euler(78, 0, 0);

        target = GameManager.main.player.transform;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        cameraTarget = new Vector3((target.position.x + locOffsetX), (target.position.y + locOffsetY), (target.position.z + locOffsetZ));
        transform.position = Vector3.Slerp(transform.position, cameraTarget, followSpeed);
    }
	
}
