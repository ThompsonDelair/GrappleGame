using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIFunctions : MonoBehaviour
{
    [SerializeField] GameObject deathUI;

    // Start is called before the first frame update
    void Start()
    {
        if (deathUI == null) {
            deathUI = GameObject.Find("DeathUI");
        }
        
        
        deathUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateDeathUI() {
        deathUI.SetActive(true);
    }

    public void Quit() {
        Application.Quit();
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
