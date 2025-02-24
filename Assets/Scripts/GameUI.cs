using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_Text flagsLeftText;
    public TMP_Text winLoseText;

    public bool timerActive = false;
    public float time;

    public GameLogic gameLogic;

    void Start()
    {
        winLoseText.text = "";
        flagsLeftText.text = gameLogic.mineCount.ToString();

        time = 0;
        timerActive = true;
    }

    void Update()
    {
        flagsLeftText.text = gameLogic.flagCount.ToString();

        if(gameLogic.gameOver)
        {
            timerActive = false;

            if(gameLogic.wonGame == true)
            {
                winLoseText.text = "You Win!";
            }

            else
            {
                winLoseText.text = "You hit a mine!\n Try again next time.";
            }
        }
        
        if (timerActive)
        {
            time += Time.deltaTime;
            displayTimer(time, timerText);
        }
    }

    private void displayTimer(float time, TMP_Text timerText)
    {
        time += 1;

        float mins = Mathf.FloorToInt(time / 60);
        float secs = Mathf.FloorToInt(time % 60);

        timerText.text = string.Format("{0:00}:{1:00}", mins, secs);
    }
}
