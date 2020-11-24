using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour {

    public static ParticleEmitter current;

    void Awake() {
        // Singleton implementation
        if (current == null) {
            current = this;
        } else {
            Destroy(this.gameObject);
        }
    }

    // Spawn a particle effect at given position. Destroy after duration comletion.
    public void SpawnParticleEffect(GameObject effect, Vector3 position, Quaternion rotation) {
        GameObject newEffect = Instantiate(effect, position, rotation);
        DestroyParticleAfterDuration(newEffect);
    }

    // Spawn a particle effect parented to some other GameObject, given position offset. Destroy after duration comletion.
    public void SpawnParticleEffect(Transform parent, GameObject effect, Vector3 positionOffset) {
        GameObject newEffect = Instantiate(effect, parent);
        newEffect.transform.position = positionOffset;

        DestroyParticleAfterDuration(newEffect);
    }

    // This function iterates through children and find longest particle duration.
    private void DestroyParticleAfterDuration(GameObject particle) {
        float duration = particle.GetComponent<ParticleSystem>().main.duration;
        
        // We want to check all children of the particle, which will be sub-particle systems.
        //      If one child is an effect that has a longer duration than the main system, we use the child's duration as the timer.
        float childDuration = duration;

        // For each child of main particle system...
        foreach(Transform child in particle.transform) {
            // 1. Check for a valid system
            ParticleSystem subSystem = child.GetComponent<ParticleSystem>();
            if (subSystem == null) {
                // If none is found, loop to next child.
                continue;
            }

            // 2. Compare child duration to current max. If higher, replace duration.
            childDuration = subSystem.main.duration;
            if (childDuration > duration) {
                duration = childDuration;
            }
        }

        // Destroy particle after max duration of system
        Destroy(particle, duration);
    }
}
