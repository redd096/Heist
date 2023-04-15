using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : redd096.UIManager
{
    [Header("Timer")]
    [SerializeField] GameObject countdownObject = default;
    [SerializeField] TextMeshProUGUI countdownBeforeStart = default;
    [SerializeField] TextMeshProUGUI timerText = default;
    [SerializeField] int secondsToStartBlink = 10;
    [SerializeField] float blinkTime = 0.5f;
    [SerializeField] int increaseFontWhenBlink = 30;

    [Header("Score")]
    [SerializeField] string scoreString = "Score: {0:000}";
    [SerializeField] TextMeshProUGUI scoreText = default;

    [Header("Text EndMenu")]
    [SerializeField] GameObject textOnWin = default;
    [SerializeField] GameObject textOnFinishTime = default;

    [Header("Score Tab EndMenu")]
    [SerializeField] string scoreTabString = "{0:000}";
    [SerializeField] float durationTextAnimation = 5f;
    [SerializeField] float waitBeforeUpdateNextText = 1;
    [SerializeField] TextMeshProUGUI scoreTabText = default;
    [SerializeField] TextMeshProUGUI timeLeftText = default;
    [SerializeField] TextMeshProUGUI finalScoreText = default;
    [SerializeField] Button[] buttonsEndMenu = default;

    Coroutine timerBlinkCoroutine;

    protected override void Start()
    {
        base.Start();

        UpdateScore(0);
        scoreTabText.text = string.Format(scoreTabString, 0);
        timeLeftText.text = $"{0:00}:{0:00}";
        finalScoreText.text = string.Format(scoreTabString, 0);
        foreach (Button button in buttonsEndMenu)
            button.interactable = false;
    }

    public void UpdateCountdownBeforeStart(int timeInSeconds)
    {
        if (countdownBeforeStart == null)
        {
            if (ShowDebugLogs) Debug.Log("Missing countdown text on UIManager");
            return;
        }

        //set countdown
        countdownBeforeStart.text = timeInSeconds.ToString();
    }

    public void ShowCountdown(bool show)
    {
        if (countdownObject == null)
        {
            if (ShowDebugLogs) Debug.Log("Missing countdown object on UIManager");
            return;
        }

        //show countdown object
        countdownObject.SetActive(show);
    }

    public void UpdateTimer(int timeInSeconds)
    {
        if (timerText == null)
        {
            if (ShowDebugLogs) Debug.Log("Missing timer text on UIManager");
            return;
        }

        //set timer minutes:seconds
        int minutes = timeInSeconds / 60;
        int seconds = timeInSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";

        //blink and increase size when left low time
        if (timeInSeconds < secondsToStartBlink && timerBlinkCoroutine == null)
        {
            timerText.fontSize += increaseFontWhenBlink;
            Vector2 sizeDelta = timerText.GetComponent<RectTransform>().sizeDelta;
            timerText.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y + increaseFontWhenBlink);
            timerBlinkCoroutine = StartCoroutine(TimerBlinkCoroutine());
        }
    }

    IEnumerator TimerBlinkCoroutine()
    {
        float time = Time.time;
        while (true)
        {
            timerText.alpha = 0;
            time += blinkTime;
            yield return new WaitUntil(() => Time.time > time);

            timerText.alpha = 1;
            time += blinkTime;
            yield return new WaitUntil(() => Time.time > time);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText == null)
        {
            if (ShowDebugLogs) Debug.Log("Missing score text on UIManager");
            return;
        }

        scoreText.text = string.Format(scoreString, score);
    }

    public void UpdateEndMenuText(bool isWin)
    {
        if (textOnWin == null || textOnFinishTime == null)
        {
            if (ShowDebugLogs) Debug.Log("Missing text endMenu on UIManager");
            return;
        }

        textOnWin.SetActive(isWin);
        textOnFinishTime.SetActive(isWin == false);
    }

    public void ShowScoreTab(int score, int timeLeftInSeconds, int finalScore)
    {
        scoreTabText.text = string.Format(scoreTabString, 0);
        timeLeftText.text = $"{0:00}:{0:00}";
        finalScoreText.text = string.Format(scoreTabString, 0);
        foreach (Button button in buttonsEndMenu)
            button.interactable = false;
        StartCoroutine(TabScoreCoroutine(score, timeLeftInSeconds, finalScore));
    }

    IEnumerator TabScoreCoroutine(int score, int timeLeftInSeconds, int finalScore)
    {
        //score
        int s = 0;
        float timeBetweenUpdates = durationTextAnimation / score;
        while (s < score)
        {
            s++;
            scoreTabText.text = string.Format(scoreTabString, s);
            yield return new WaitForSeconds(timeBetweenUpdates);
        }
        yield return new WaitForSeconds(waitBeforeUpdateNextText);

        //time left
        s = 0;
        timeBetweenUpdates = durationTextAnimation / timeLeftInSeconds;
        while (s < timeLeftInSeconds)
        {
            s++;
            //minutes:seconds
            int minutes = s / 60;
            int seconds = s % 60;
            timeLeftText.text = $"{minutes:00}:{seconds:00}";
            yield return new WaitForSeconds(timeBetweenUpdates);
        }
        yield return new WaitForSeconds(waitBeforeUpdateNextText);

        //final score
        s = 0;
        timeBetweenUpdates = durationTextAnimation / finalScore;
        while (s < finalScore)
        {
            s++;
            finalScoreText.text = string.Format(scoreTabString, s);
            yield return new WaitForSeconds(timeBetweenUpdates);
        }
        yield return new WaitForSeconds(waitBeforeUpdateNextText);

        //activate buttons
        foreach (Button button in buttonsEndMenu)
            button.interactable = true;
    }
}
