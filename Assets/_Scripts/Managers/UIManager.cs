using UnityEngine;
using TMPro;

public class UIManager : redd096.UIManager
{
    [Header("Timer")]
    [SerializeField] GameObject countdownObject = default;
    [SerializeField] TextMeshProUGUI countdownBeforeStart = default;
    [SerializeField] TextMeshProUGUI timerText = default;

    [Header("Text EndMenu")]
    [SerializeField] GameObject textOnWin = default;
    [SerializeField] GameObject textOnFinishTime = default;

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
}
