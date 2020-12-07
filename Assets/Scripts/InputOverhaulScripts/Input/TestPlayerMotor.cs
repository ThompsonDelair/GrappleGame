using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// REQUIREMENTS:
[RequireComponent(typeof(InputBuffer))]


// This class is something Kurtis used to test the new Input System, as well as animations.
//      While we likely won't use this directly, we may be able to derive what type of info
//      will be required to animate the player, as well as some applications for the new Input System.
public class TestPlayerMotor : MonoBehaviour {

    // Component References
    protected InputBuffer buffer;
    [SerializeField] protected Animator playerAnimator;
    [SerializeField] protected Camera mainCamera;
    [SerializeField] private GameObject playerModel;

    // Move Variables
    public float moveSpeed = 5f;
    public float speedModifier = 1f; // When raising or lowering payer speed, it's better to modify this than the base move speed.

    // Rotation Variables
    public float turnSpeed = 0.1f;
    private Vector3 MoveVector = Vector3.zero; // Movement Vector, assigned based on user input.
    private Vector3 playerDirection = new Vector3();

    // Start is called before the first frame update
    void Start() {
        buffer = this.GetComponent<InputBuffer>();
        playerAnimator = this.GetComponent<Animator>();
        mainCamera = Camera.main;
    }


    // Update is called once per frame
    void FixedUpdate() {
        ManageMovement();
        ManageRotation();
    }

    // This function is used to move the player.
    // It'll likely be replaced, so it's very simple and bare-bones.
    public void ManageMovement() {
        Vector3 input =  CreateNormalizedVector(buffer.GetMovementVector());
		MoveVector = input;

        playerDirection = CreateNormalizedVector(buffer.GetRotationVector());

        // 2. If inputVector is non-zero, update transform position. 
        if (MoveVector.sqrMagnitude > Mathf.Epsilon) {
            // Set animator variable.
            playerAnimator.SetBool("Running", true);

            // Update transform translate
            transform.Translate(MoveVector * Time.deltaTime * moveSpeed, Space.World);

        } else {

            // Set animator variable.
            playerAnimator.SetBool("Running", false);
            
        }
    }


    // This function is used to rotate the player towards their desired direction.
    //      If there's a controller input, we'll use the Right Thumbstick as a target.
    //      Otherwise, we'll look towards the mouse cursor.
    public void ManageRotation() {
        // 1. Create the player direction vector
        Vector3 playerDirection = new Vector3();

        // This will be used to manage the rotation if the player is simply running forward.
        //      In lieu of an explicit rotation, we want to turn forward and play a forward running animation.
        Vector3 moveDirection = CreateNormalizedVector(buffer.GetMovementVector());;
        
        // 2. We can check the input method using the InputBuffer component.
        if (buffer.GamepadInputActive()) {

            // Controller Rotations - Controller is real simple. If any input is being held on the Rotation Stick (in this case Right Thumbstick),
            //      We rotate towards that. The rate at which we turn can be defined in the inspector, but usually hovers close to 0.1f.
            playerDirection = CreateNormalizedVector(buffer.GetRotationVector());
            if (playerDirection.sqrMagnitude > 0.0f) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerDirection, Vector3.up), turnSpeed);
            
            // If there's no rotation on the Right Thumbstick, we want to check the Movement Direction. If it's non-zero, we'll turn towards that instead.
            } else if (MoveVector != Vector3.zero && !(playerDirection.sqrMagnitude > 0.0f)) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), turnSpeed);
            }
        
        } else {

            // Mouse Rotations - A way I did this is to cast a ray from the main camera to a Plane we create in world space.
            //      We can use the point of intersection between the ray and the plane as a target to look towards.

            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayLength;
            
            // Here's the ray. We cast from the camera to the mouse position.
            Ray cameraRay = mainCamera.ScreenPointToRay(buffer.GetMousePosition());

            // If we intersect with the ground plane, we can get it's point of intersection and look towards it.
            if(groundPlane.Raycast(cameraRay, out rayLength)) {
                playerDirection = cameraRay.GetPoint(rayLength);
                transform.LookAt(new Vector3(playerDirection.x, transform.position.y, playerDirection.z));

                // We can see the ray in the scene view.
                Debug.DrawLine(cameraRay.origin, playerDirection, Color.blue);
            }
        }
        
        // Get current move vectors for animation.
        //      This Inverted so that the axis (forward / backward, for instance) is relative to the way the character is facing.
        float moveX = transform.InverseTransformDirection(moveDirection).x * speedModifier;
        float moveZ = transform.InverseTransformDirection(moveDirection).z * speedModifier;

        playerAnimator.SetFloat("MoveX", moveX, .05f, Time.deltaTime);
        playerAnimator.SetFloat("MoveZ", moveZ, .05f, Time.deltaTime);

        // See what vector has a greater value to determine speed of motion.
        //      It's a value between 0 and 1 such that the animation is relative to max speed.
        if (Mathf.Abs(moveX) > Mathf.Abs(moveZ)) {
            playerAnimator.SetFloat("Speed",  Mathf.Abs(moveX), .05f, Time.deltaTime);
        } else {
            playerAnimator.SetFloat("Speed", Mathf.Abs(moveZ), .05f, Time.deltaTime);
        }

	}

    public Vector3 CreateNormalizedVector(Vector2 input) {
		Vector3 dir = Vector3.zero;

		dir.x = input.x;
		dir.z = input.y;

		if (dir.sqrMagnitude > 1) {
			dir.Normalize();
		}

		return dir;
	}
}
