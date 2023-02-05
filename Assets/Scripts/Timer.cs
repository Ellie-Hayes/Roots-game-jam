using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Timer : MonoBehaviour
{
    public float time;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI finalTime;
    MainManager mainManagerScript;

    private void Start()
    {
        mainManagerScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MainManager>();
    }
    private void Update()
    {
        if (!mainManagerScript.paused && !mainManagerScript.won)
        {
            time += Time.deltaTime;
            DisplayTime(time);
        }
       
        
    }
    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliSeconds = (timeToDisplay % 1) * 1000;
        timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds);
    }

}
