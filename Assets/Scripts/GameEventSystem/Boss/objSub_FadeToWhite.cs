using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class objSub_FadeToWhite : ObjectiveSubscriber {
    [SerializeField] Image fadeEffect;
    [SerializeField] float duration = 5f;
    [SerializeField] AudioClip finalGrowl;
    [SerializeField] AudioClip finalExplosion;
    [SerializeField] GameObject explosion;
    protected override void OnObjectiveCompletion() {
        
        StartCoroutine(FadeToWhiteOverDuration());
    }

    protected IEnumerator FadeToWhiteOverDuration() {
        float elapsedTime = 0;
        explosion.SetActive(true);
        AudioSource source = this.GetComponent<AudioSource>();
        source.PlayOneShot(finalExplosion);
        source.PlayOneShot(finalGrowl);
        EffectController.main.CameraShake(0.4f, duration*2);

        // Explosion is happening, disable the game manager's player input
        GameManager.main.player.invulnerable = true;
        GameManager.main.transform.GetComponent<PlayerInput>().enabled = false;
        GameManager.main.player.transform.GetComponent<StateDriver>().active = false;
        GameManager.main.transform.GetComponent<InputBuffer>().enabled = false;

        Vector3 targetScale =  explosion.transform.localScale*1000;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;

            if (fadeEffect != null) {
                
                Color c = fadeEffect.color;
                c.a = Mathf.Clamp01(elapsedTime / duration);
                fadeEffect.color = c;

                explosion.transform.localScale = Vector3.Lerp (explosion.transform.localScale, targetScale, 0.2f * Time.deltaTime);

                yield return null;
                
            }

            
        }
        
    }
}
