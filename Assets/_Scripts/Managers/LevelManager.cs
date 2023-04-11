using redd096.Attributes;
using redd096.GameTopDown2D;
using redd096.StateMachine.StateMachineRedd096;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] int secondsBeforeStartTimer = 3;
    [SerializeField] int timerInSeconds = 90;
    [SerializeField] bool activePlayersAfterCountdown = true;
    [EnableIf("activePlayersAfterCountdown")][SerializeField] string firstStatePlayers = "NormalState";

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

        //set state to every player
        if (activePlayersAfterCountdown)
        {
            foreach (Character character in FindObjectsOfType<Character>())
                if (character.CharacterType == Character.ECharacterType.Player)
                    character.GetComponentInChildren<StateMachineRedd096>().SetState(firstStatePlayers);
        }
    }

    void OnFinishTimer()
    {
        //show end menu
        GameManager.uiManager.UpdateTimer(0);
        state = EStateLevelManager.endGame;
        GameManager.uiManager.EndMenu(true);
    }

    public void TryActivateCharacter(Character character)
    {
        if (state == EStateLevelManager.game)
            character.GetComponentInChildren<StateMachineRedd096>().SetState(firstStatePlayers);
    }
}
