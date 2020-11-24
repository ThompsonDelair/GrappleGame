using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

// REQUIREMENTS:

public class EffectController : MonoBehaviour
{
    public static EffectController main;
    protected IEnumerator currentStop;
    protected IEnumerator currentRumble;
    protected Camera targetCamera;

    void Awake() {
        if (main == null) {
            // DontDestroyOnLoad(this);
            main = this;

        } else {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        targetCamera = Camera.main;
    }

    public virtual void HitStop(float duration) {
        if (currentStop != null) {
            StopCoroutine(currentStop);
        }

        currentStop = StopForDuration(duration);

        StartCoroutine(currentStop);
    }

    protected IEnumerator StopForDuration(float duration) {
        Time.timeScale = 0f;

        while (duration >= 0.0f) {
            duration -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Time.timeScale = 1f;
        
        currentStop = null;
    }

    public virtual void CameraShake(float duration, float magnitude) {
        if (targetCamera != null) {
            StartCoroutine(ShakeForDuration(duration, magnitude));
        }
    }

    protected IEnumerator ShakeForDuration(float duration, float magnitude) {
        Quaternion originalRot = targetCamera.transform.localRotation;

        while (duration >= 0.0f) {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            targetCamera.transform.localPosition = new Vector3(x, y, z);

            duration -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        targetCamera.transform.localPosition = Vector3.zero;
    }

    public virtual void ControllerRumble(float duration, float lowFreqMagnitude = 1f, float highFreqMagnitude = 1f) {

        InputBuffer input = this.GetComponent<InputBuffer>();
        if (input != null && input.GamepadInputActive()) {

            if (currentRumble != null) {
                StopCoroutine(currentRumble);
            }

            currentRumble = RumbleForDuration(duration, lowFreqMagnitude, highFreqMagnitude);
            StartCoroutine(currentRumble);
        }
    }

    protected IEnumerator RumbleForDuration(float duration, float lowFreqMagnitude, float highFreqMagnitude) {
        Gamepad.current.SetMotorSpeeds(1f, 1f);

        while (duration >= 0.0f) {
            duration -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        InputSystem.ResetHaptics();
    }
}
