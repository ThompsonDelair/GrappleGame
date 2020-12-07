using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_TrackMixer : ObjectiveSubscriber {

    protected AudioSource source;
    [SerializeField] List<AudioClip> tracks;
    protected int songIndex = 0;

    void Start() {
        gameManager = FindObjectOfType<GameManager>();
        GameEventDirector.current.onObjectiveCompletion += CheckObjectiveId; // This is setting what function will be called on event dispatch.

        source = this.GetComponent<AudioSource>();

        // Start playing the first track in the list.
        if (tracks.Count > 0) {
            source.clip = tracks[songIndex];

            if (source.clip != null) {
                source.Play();
            } else {
                source.Stop();
            }
            
        }


    }

    protected override void OnObjectiveCompletion() {
        // Start playing the first track in the list.
        if (tracks.Count > 0 && songIndex < tracks.Count) {
            songIndex++;
            source.clip = tracks[songIndex];
        }
        
        if (source.clip != null) {
            source.Play();
        } else {
            source.Stop();
        }
    }
    
}
