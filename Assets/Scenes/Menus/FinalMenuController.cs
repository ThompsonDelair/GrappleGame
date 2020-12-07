using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script is used only in the post-game title menu to time the fade-in to the music.
public class FinalMenuController : MonoBehaviour
{
    [SerializeField] GameObject eventSystem;
    [SerializeField] Image fadeOverlay;
    [SerializeField] Text resultText;
    [SerializeField] protected float delayBeforeFadeIn = 2f;

    [SerializeField] protected float fadeDuration = 3f;

    // Start is called before the first frame update
    void Start() {
        if (resultText != null && ScoreDirector.main != null) {

            string result = "";
            result += ( "> Clear Time  " + ScoreDirector.main.FinalTime + "\n\n");
            result += ( "Total Kills : " + ScoreDirector.main.TotalKills + "\n");
            result += ( "Total Deaths : " + ScoreDirector.main.TotalDeaths + "\n");

            resultText.text = result;
        }

        StartCoroutine(FadeInAfterDelay());
    }

    protected IEnumerator FadeInAfterDelay() {
        yield return new WaitForSeconds(delayBeforeFadeIn);

        eventSystem.SetActive(true);
        Color color = fadeOverlay.color;
        float elapsedTime = fadeDuration;
        while (elapsedTime > 0) {

            // Example
            // Color c = fadeEffect.color;
            // c.a = Mathf.Clamp01(elapsedTime / duration);
            // fadeEffect.color = c;

            // Actual
            elapsedTime -= Time.deltaTime;
            color = fadeOverlay.color;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeOverlay.color = color;

            yield return null;
        }
    }
}
