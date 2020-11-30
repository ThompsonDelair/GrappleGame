using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHazardCollision : MonoBehaviour
{
    //float playerNormalSpeed; 

    // Start is called before the first frame update
    void Start()
    {
        //playerNormalSpeed = GameManager.main.playerSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(UnityEngine.Collider other) {
        //GameManager.main.hazardColliding = true;
        
    }

    //private void OnTriggerEnter(Collider other) {
    //    GameManager.main.hazardColliding = true;
    //    Debug.Log("Colliding started");
    //}

    //private void OnTriggerExit(Collider other) {
    //    GameManager.main.playerSpeed = playerNormalSpeed;
    //    Debug.Log("Colliding end");
    //}
}
