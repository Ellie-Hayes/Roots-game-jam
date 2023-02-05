using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json.Linq;

public class MainManager : MonoBehaviour
{
    public bool paused;
    public bool won;
    public float finalTime;
    public GameObject pausedCanvas;

    public GameObject winCanvas;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI MedalText;
    public TextMeshProUGUI MedalDescription;

    public GameObject[] medals; //0 none, 1 bronxe, 2 silver, 3 gold
    Timer timerscript;

    public AudioClip pausePop;

    [SerializeField]
    AudioSource soundEffectSource;

    // Start is called before the first frame update
    void Start()
    {
        timerscript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Timer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (won)
        {
            finalTime = timerscript.time;
            winCanvas.SetActive(true);
            DisplayTime(finalTime);
            GetMedal();

            PlayerPrefs.SetFloat("Highscore", finalTime);

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            soundEffectSource.PlayOneShot(pausePop, 0.8F);
            paused = !paused;
            pausedCanvas.SetActive(!pausedCanvas.activeSelf);
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliSeconds = (timeToDisplay % 1) * 1000;
        finalTimeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds);
    }

    void GetMedal()
    {
        if (finalTime > 960) //16 mins
        {
            medals[0].SetActive(true);
            MedalText.text = "No medal";
            MedalDescription.text = "You either really took your time or we did a great job puzzle making";
        }
        else if (finalTime > 720) // 12 mins
        {
            medals[1].SetActive(true);
            MedalText.text = "Bronze medal";
            MedalDescription.text = "You were quicker than lily, but not quick enough. You were still pretty bad";
        }
        else if (finalTime > 480) // 7 mins
        {
            medals[2].SetActive(true);
            MedalText.text = "Silver medal";
            MedalDescription.text = "My hand cramps and I bet yours does too";
        }
        else
        {
            medals[3].SetActive(true);
            MedalText.text = "gold medal";
            MedalDescription.text = "As my dad says, speedy gonzales";
        }

    }
}
