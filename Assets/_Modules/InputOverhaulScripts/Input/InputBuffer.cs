using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

// For consistent references of input names from outside components.
public static class InputName {
    // Weapon Actions
	public static string Fire { get { return "Fire"; } }
    public static string Grapple { get { return "Grapple"; } }

    public static string Quit { get { return "Quit"; } }

    //public const string Quit = "Quit";

}

// Input Buffer is made to be a resource that is polled by other components.
//      Specifically, it is designed to handle player combat-related inputs, and attempts
//      to simplify access to specific Action Inputs, such as Movement Vectors and Attack Triggers.

// REQUIREMENTS:
[RequireComponent(typeof(PlayerInput))]

public class InputBuffer : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private PlayerInput inputComponent;

    [Header("Logistic Fields")]
    [SerializeField] private float bufferTime = 0.1f; // How long a trigger is stored and considered "active"
    [SerializeField] private bool gamepadInput = true; // Set if a gamepad is detected as the last input device
    public bool GamepadInputActive() { return gamepadInput; }

    // Subscribed Vectors
    [Header("Vectors")]
    [SerializeField] private Vector2 movementVector;
    public Vector2 GetMovementVector() { return movementVector; }

	[SerializeField] private Vector2 rotationVector;
    public Vector2 GetRotationVector() { return rotationVector; }

	[SerializeField] private Vector2 mousePosition;
    public Vector2 GetMousePosition() { return mousePosition; }

    // Both Combat Action Triggers and Holds are stored in a Lookup Table.
    //      Access to the values in the table are controlled within the InputBuffer class.
    //      Actions are NOT added to the table until their first invocation.
    //      ONLY explicitly subscribed actions are added to the table.
    private Dictionary<string, bool> triggeredActions;
    private Dictionary<string, bool> heldActions;

    // Returns the value of corresponding action.
    //      Returns FALSE if the action does not exist in the table.
    //      Optionally, may consume the trigger to reset it's value
    public bool ActionTriggered(string actionName, bool consumeTrigger = false) {
        bool value;
        if (!triggeredActions.TryGetValue(actionName, out value)) {
            return false;
        }

        if (consumeTrigger) {
            ConsumeTrigger(actionName);
        }

        return value;
    }

    public void ConsumeTrigger(string actionName) {
        bool value;
        if (!triggeredActions.TryGetValue(actionName, out value)) {
            return;
        }

        if (value) {
            triggeredActions[actionName] = false;
        }
    }

    // Returns the value of corresponding action.
    //      Returns FALSE if the action does not exist in the table.
    public bool ActionHeld(string actionName) {
        bool value;
        if (!heldActions.TryGetValue(actionName, out value)) {
            return false;
        }
        return value;
    }

    // Awake 
    void Awake() {
        // Component setup
        inputActions = new PlayerInputActions();
        inputComponent = this.GetComponentInParent<PlayerInput>();

        // Buffer init
        triggeredActions = new Dictionary<string, bool>();
        heldActions = new Dictionary<string, bool>();
        
        // Subscribed actions
        inputActions.PlayerControls.Fire.performed += context => BufferInput(context);
        inputActions.PlayerControls.Grapple.performed += context => BufferInput(context);
        inputActions.PlayerControls.Quit.performed += context => BufferInput(context);

    }

    // These pre-defined functions are broadcast from the PlayerInput component.
    void OnMovement(InputValue value) {
        movementVector = value.Get<Vector2>();
    }

    void OnRotation(InputValue value) {
        rotationVector = value.Get<Vector2>();
    }

    void OnMousePosition(InputValue value) {
        mousePosition = value.Get<Vector2>();
    }

    void OnControlsChanged() {
        // Clear previous input data
        // movementVector = Vector2.zero;
        // rotationVector = Vector2.zero;
        
        CheckInputSource();
    }

    // Determines whether gamepad currently being used or not.
    private void CheckInputSource() {
        if (inputComponent.currentControlScheme == "Gamepad") {
            gamepadInput = true;
        } else {
            gamepadInput = false;
        }
    }

    // FixedUpdate is Framerate Independant
    void FixedUpdate() {
        // Check to see if any actions are held this frame
        UpdateHeldAction(InputName.Fire, (inputActions.PlayerControls.Fire.ReadValue<float>() != 0));
        UpdateHeldAction(InputName.Grapple, (inputActions.PlayerControls.Fire.ReadValue<float>() != 0));

    }

    // Take in the current action's context, and buffer it's trigger for some time.
    // This function increases how responsive combat inputs will feel.
    private void BufferInput(InputAction.CallbackContext context) {
        // Make sure the Input is available in the lookup table
        if (!triggeredActions.ContainsKey(context.action.name)) {
            triggeredActions.Add(context.action.name, false);
        }

        // Check if the trigger is already active before starting a coroutine
        if (!triggeredActions[context.action.name]) {
            StartCoroutine(Buffer(context.action.name, bufferTime));
        }
    }

    // Check to see if the given Action's input is being held down.
    private void UpdateHeldAction(string actionName, bool value) {
        // Make sure the Action is available in the lookup table
        if (!heldActions.ContainsKey(actionName)) {
            heldActions.Add(actionName, false);
        }

        heldActions[actionName] = value;
    }

    // Activates the designated Action Trigger for some amount of time.
    private IEnumerator Buffer(string actionName, float bufferTime) {
        triggeredActions[actionName] = true;

        while (bufferTime >= 0.0f) {
            bufferTime -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        triggeredActions[actionName] = false;
    }

    // New Input System requires these function defintions.
    private void OnEnable() {
		inputActions.Enable();
	}

	private void OnDisable() {
		inputActions.Disable();
	}
}
