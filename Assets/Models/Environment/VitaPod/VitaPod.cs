using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FieldOfView))]
public class VitaPod : MonoBehaviour
{
    protected FieldOfView fov;

    [Header("Vita Pod Configs")]
    public float chargeTime = 5f;
    protected float currentTime = 0f;
    public int healthCapacity = 4;
    protected int currentInterval = 1;

    [Header("Visual Configs")]
    [SerializeField] protected Light activeLight;
    [SerializeField] protected MeshRenderer fieldVisualization;
    
    private bool active = true;

    // Start is called before the first frame update
    void Start() {
        fov = this.GetComponent<FieldOfView>();
    }

    // VitaPod has been implemented as an Objective Tracker.
    // As such, it will dispatch it's event when depleted.
    void Update() {
        if (active) {
            if ( GameManager.main.player.health == GameManager.main.player.stats.maxHP) {
                gameObject.GetComponent<AudioSource>().Pause();
                return;
            }

            if (fov.WithinRadius(GameManager.main.player.transform.position, fov.ViewRadius) && currentTime < chargeTime) {
                currentTime += Time.deltaTime;

                // For sound
                /*GameObject player = GameManager.main.player.transform.gameObject;
                if (player.GetComponent<AudioSource>() != null) // Check if player has audio source
                {
                    AudioSource playerAudio = player.GetComponent<AudioSource>();
                    if (playerAudio.clip == AudioClips.singleton.HPCharge)
                    {

                    }
                }*/ // WAS GOING TO DO AUDIO ON PLAYER, FOR THIS INSTANCE MAKES MORE SENSE TO HAVE AUDIO SOURCE ON THE PREFAB OF VIY

                if (!gameObject.GetComponent<AudioSource>().isPlaying)
                {
                    gameObject.GetComponent<AudioSource>().Play(); // Special case playing without soundmanager, no need to track outside of this context
                }
            }
            else
            {
                gameObject.GetComponent<AudioSource>().Pause();
            }

            fov.ViewAngle = (currentTime/chargeTime) * 360 > 360 ? 360 : (currentTime/chargeTime) * 360;
            fov.DrawFieldOfView();

            // Get the intervals that allow health regen.
            float targetInterval = (chargeTime / healthCapacity) * currentInterval;

            if (currentTime >= targetInterval) {
                DamageSystem.RestoreHealth(GameManager.main.player, 1);
                currentInterval++;
                // Each increment plays one shot on increment
                SoundManager.PlayOneClipAtLocation(AudioClips.singleton.HPIncrement, GameManager.main.player.position2D, 6f);
            }

            if (currentTime >= chargeTime) {
                active = false;
                gameObject.GetComponent<AudioSource>().Pause();

                if (activeLight != null) {
                    activeLight.gameObject.SetActive(false);
                }

                if (fieldVisualization != null) {
                    fieldVisualization.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            gameObject.GetComponent<AudioSource>().Pause();
        }
    }
}
