using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadMainMenu : MonoBehaviour
{
    [SerializeField] GameObject victoryMessage;
    [SerializeField] GameObject resultMessage;
    // Start is called before the first frame update
    AudioSource source;
    bool secondDrumBeat = false;
    void Start()
    {
        source = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > 6f && !secondDrumBeat) {
            victoryMessage.SetActive(false);
            DisplayResultsFromScore();
            source.PlayOneShot(source.clip);
            secondDrumBeat = true;
        }

        if (Time.timeSinceLevelLoad > 17f)
            SceneManager.LoadScene("MainMenuCompletion");
    }

    private void DisplayResultsFromScore() {
        if (resultMessage != null) {
            string result = "";
            result += ( "> Clear Time  " + ScoreDirector.main.FinalTime + "\n\n");
            result += ( "Total Kills : " + ScoreDirector.main.TotalKills + "\n");
            result += ( "Total Deaths : " + ScoreDirector.main.TotalDeaths + "\n");

            resultMessage.GetComponent<Text>().text = result;

            resultMessage.SetActive(true);
        }
    }
}
