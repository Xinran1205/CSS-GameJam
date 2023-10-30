using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class CountdownTimer : MonoBehaviour
{
    public float timeRemaining = 60; // 倒计时的时间，例如10秒
    public bool timerIsRunning = false;
    public TextMeshProUGUI timeText;
    public GameObject finalpage;

    public GameObject Grass;

    
     void Start()
    {
        timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                EndGame();
            }
        }

        UpdateFloor();
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void UpdateFloor()
    {
        if (timeRemaining<=10)
        {
            GameObject child1 = Grass.transform.Find("Grass (1)").gameObject;
            child1.SetActive (false);
        }else if (timeRemaining <= 20) 
        {
            GameObject child2 = Grass.transform.Find("Grass (2)").gameObject;
            child2.SetActive(false);
        }
        else if (timeRemaining <= 30)
        {
            GameObject child3 = Grass.transform.Find("Grass (3)").gameObject;
            child3.SetActive(false);
        }
        else if (timeRemaining <= 40)
        {
            GameObject child4 = Grass.transform.Find("Grass (4)").gameObject;
            child4.SetActive(false);
        }

    }

    void EndGame()
    {
        finalpage.SetActive(true);
    }
}
