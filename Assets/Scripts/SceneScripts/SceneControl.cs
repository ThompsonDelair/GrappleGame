using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControl : MonoBehaviour
{
 
    public float levelLoadDelay = 5f;
    
    [SerializeField] static int startLevelIndex = 1;
    [SerializeField] static int endLevelIndex = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMessageThenNextLevel(string s, float durationOverride = 0f) {
        GameObject.Find("LevelCompleteText").GetComponent<Text>().text = s;
        StartCoroutine(WaitToLoad(durationOverride));
    }

    IEnumerator WaitToLoad(float durationOverride = 0f) {
        if (durationOverride > 0) {
            yield return new WaitForSeconds(durationOverride);
        } else {
            yield return new WaitForSeconds(levelLoadDelay);
        }
        
        LoadNextLevel();
    }

    public static void LoadNextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public static void LoadLevel(int index) {
        // If loading the first level, begin tracking the score
        if (index == startLevelIndex) {
            ScoreDirector.main.StartNewGame();
        } else if (index == endLevelIndex) {
            ScoreDirector.main.EndGame();
        }

        SceneManager.LoadScene(index);
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
