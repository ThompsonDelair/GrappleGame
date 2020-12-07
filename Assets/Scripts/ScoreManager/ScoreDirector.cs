using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreDirector : MonoBehaviour
{
    public static ScoreDirector main; 

    [SerializeField] private bool gameStarted = false;

    [Header("Total Deaths")]
    [SerializeField] int totalDeaths;
    public int TotalDeaths { get { return totalDeaths; } }

    [Header("Total Kills")]
    [SerializeField] int totalKills;
    public int TotalKills { get { return totalKills; } }

    [Header("Time")]
    [SerializeField] float totalPlayTime;

    [SerializeField] float minutes;
    [SerializeField] float seconds;
    [SerializeField] float milliseconds;

    public string FinalTime { 
        get {
            string result = "";
            result += (string.Format("{0:00}", minutes) + " : " );
            result += (string.Format("{0:00}", seconds) + " : " );
            result += (string.Format("{0:00}", milliseconds));

            return result;
        }
    }

    void Awake() {
        if (main == null) {
            main = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
        
    }
    // Start is called before the first frame update
    void Start() {
        totalPlayTime = 0f;
        totalDeaths = 0;
        totalKills = 0;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (gameStarted) {
            totalPlayTime += Time.fixedDeltaTime;

            minutes = Mathf.Floor(totalPlayTime / 60);
            seconds = Mathf.FloorToInt(totalPlayTime % 60);
            milliseconds = Mathf.FloorToInt(((totalPlayTime - Mathf.FloorToInt(totalPlayTime)) * 100));
        }
    }

    public void StartNewGame() {
        totalPlayTime = 0f;
        totalDeaths = 0;
        totalKills = 0;
        gameStarted = true;
    }

    public void EndGame() {
        gameStarted = false;
    }

    public void AddDeath() {
        totalDeaths++;
    }

    public void AddKill() {
        totalKills++;
    }


}
