using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] int secondsBeforeStartTimer = 3;
    [SerializeField] int timerInSeconds = 90;

    enum EStateLevelManager { countdown, game, endGame}

    EStateLevelManager state = EStateLevelManager.countdown;
    float timer;

    private void Start()
    {
        timer = Time.time + secondsBeforeStartTimer;
        GameManager.uiManager.UpdateCountdownBeforeStart(secondsBeforeStartTimer);
        GameManager.uiManager.UpdateTimer(timerInSeconds);
        GameManager.uiManager.ShowCountdown(true);
    }

    private void Update()
    {
        //update countdown
        if (state == EStateLevelManager.countdown)
        {
            if (Time.time > timer)
            {
                OnFinishCountdown();
                return;
            }
            GameManager.uiManager.UpdateCountdownBeforeStart(Mathf.CeilToInt(timer - Time.time));
        }
        //update timer
        else if (state == EStateLevelManager.game)
        {
            if (Time.time > timer)
            {
                OnFinishTimer();
                return;
            }
            GameManager.uiManager.UpdateTimer(Mathf.CeilToInt(timer - Time.time));
        }
    }

    void OnFinishCountdown()
    {
        //set timer
        GameManager.uiManager.ShowCountdown(false);
        state = EStateLevelManager.game;
        timer = Time.time + timerInSeconds;
    }

    void OnFinishTimer()
    {
        //show end menu
        GameManager.uiManager.UpdateTimer(0);
        state = EStateLevelManager.endGame;
        GameManager.uiManager.EndMenu(true);
    }
}
