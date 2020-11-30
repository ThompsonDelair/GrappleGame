using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControl : MonoBehaviour
{
 
    public float levelLoadDelay = 5f;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMessageThenNextLevel(string s) {
        GameObject.Find("LevelCompleteText").GetComponent<Text>().text = s;
        StartCoroutine(WaitToLoad());
    }

    IEnumerator WaitToLoad() {
        yield return new WaitForSeconds(levelLoadDelay);
        LoadNextLevel();
    }

    public static void LoadNextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public static void LoadLevel(int index) {
        SceneManager.LoadScene(index);
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
