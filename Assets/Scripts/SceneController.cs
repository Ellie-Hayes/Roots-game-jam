using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            PlayerPrefs.DeleteAll();
        }
    }
    public GameObject overlayOut; 
    public void InvokeMenu()
    {
       overlayOut.SetActive(true);
        Invoke("MenuLink", 1);
    }

    public void InvokeGame()
    {
        overlayOut.SetActive(true);
        Invoke("GameLink", 1);
    }

    public void MenuLink()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GameLink()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitLink()
    {
        Application.Quit();    
    }
}
