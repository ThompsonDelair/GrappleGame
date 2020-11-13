using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour {

    public static ParticleEmitter current;

    void Awake() {
        
        if (current == null) {
            // DontDestroyOnLoad(this);
            current = this;

        } else {
            Destroy(this.gameObject);
        }
    }

    public void SpawnParticleEffect(GameObject effect, Vector3 position) {
        GameObject newEffect = Instantiate(effect, position, Quaternion.identity);
    }

    public void SpawnParticleEffect(Transform parent, GameObject effect, Vector3 positionOffset) {
        GameObject newEffect = Instantiate(effect, parent);
        newEffect.transform.position = positionOffset;
    }
}
