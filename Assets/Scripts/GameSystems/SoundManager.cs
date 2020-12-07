using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sound manager controls playing sounds for all of the recurring sounds in game. (With the exception of 1 or 2 special cases)
/// </summary>
public static class SoundManager
{
    private static bool currentlyPlayingAmbient = false;

    // Plays clip at location using AudioSource.playClipAtPoint, destroys audio source after playing clip
    // I know it seems redundant but having easy access to the references through this class is valuable
    // ** Use when audio needs to be played in one shot at one location without moving
    public static void PlayOneClipAtLocation(AudioClip sound, Vector3 location, float volume) // Volume is very important in adjusting for diagetic sound, default diagetic sound works well with this method if volume is tuned instead of chosen arbitrarily on a clip by clip basis
    {
        AudioSource.PlayClipAtPoint(sound, location, volume); // Creates an audio source at the location then plays one shot through it with the specified clip
    }


    // Creates an audio source using the specified clip and then attaches an object containing the audio source to the calling actor's gameObject
    public static void StartClipOnActor(AudioClip sound, Actor actor, float volume, bool looping)
    {
        AudioSource audioSource;
        if (actor.transform.gameObject.GetComponent<AudioSource>() == null)
        {
            audioSource = actor.transform.gameObject.AddComponent<AudioSource>(); // attached to actor's gameobject, so that clip is played from actor location
        }
        else
        {
            audioSource = actor.transform.gameObject.GetComponent<AudioSource>();
        }
        
        audioSource.clip = sound;
        audioSource.volume = volume;
        audioSource.loop = looping;
        audioSource.Play(); // Starts the clip, will be stopped when stop function is called with the actor
    }
    public static void StopClipOnActor(AudioClip sound, Actor actor)
    {
        if (actor.transform.gameObject.GetComponent<AudioSource>() == null)
        {
            return; // No audio source to stop.
        }
        else
        {
            if(actor.transform.gameObject.GetComponent<AudioSource>().clip == sound)
            {
                actor.transform.gameObject.GetComponent<AudioSource>().Stop();
            }
            else
            {
                Debug.Log("Attempting to remove a sound that isn't on the actor's audio source");
            }
        }
    }

    // Plays a looping sound from an audio source in the player's camera.
    public static void StartAmbientSound(AudioClip sound, float volume)
    {
        AudioSource ambientAudioSource;

        // Grab a reference to the mainCamera, we want to play the audio from here since the main audio listener is here and the sound is ambient in the entire scene
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        
        if(mainCamera.GetComponent<AudioSource>() == null) // If there isn't already an audio source on the main cam, add one
        {
            ambientAudioSource = mainCamera.AddComponent<AudioSource>();
            ambientAudioSource.loop = true; // Want to loop scene's sound for entirety of scene
        }
        else
        {
            ambientAudioSource = mainCamera.GetComponent<AudioSource>();
        }

        ambientAudioSource.clip = sound;
        ambientAudioSource.volume = volume;
        ambientAudioSource.Play();
        currentlyPlayingAmbient = true;
    }

    // Quick functions to pause and resume existing ambient sound
    public static void PauseAmbientSound()
    {
        AudioSource ambientAudioSource;
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        if (mainCamera.GetComponent<AudioSource>() == null) // If there isn't already an audio source on the main cam then there is nothing to pause
        {
            return;
        }
        else // Pause the existing audio source
        {
            ambientAudioSource = mainCamera.GetComponent<AudioSource>();
            ambientAudioSource.Pause();
            currentlyPlayingAmbient = false;
        }
    }
    public static void ResumeAmbientSound()
    {
        AudioSource ambientAudioSource;
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        if (mainCamera.GetComponent<AudioSource>() == null) // If there isn't already an audio source on the main cam then there is nothing to resume
        {
            return;
        }
        else // Resume the audio that was playing
        {
            ambientAudioSource = mainCamera.GetComponent<AudioSource>();
            ambientAudioSource.Play();
            currentlyPlayingAmbient = true;
        }
    }
}














