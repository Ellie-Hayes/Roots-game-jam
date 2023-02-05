using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class MainMenuHighscore : MonoBehaviour
{
    public TextMeshProUGUI bestTimeText;
    float bestTime;

    private void Start()
    {
        bestTime = PlayerPrefs.GetFloat("Highscore");
        DisplayTime(bestTime);
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliSeconds = (timeToDisplay % 1) * 1000;
        bestTimeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds);
    }
}
