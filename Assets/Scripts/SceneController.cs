using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void MenuLink()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GameLink()
    {
        SceneManager.LoadScene("Game");
    }
}
