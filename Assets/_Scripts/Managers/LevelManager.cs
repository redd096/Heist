using redd096.Attributes;
using redd096.GameTopDown2D;
using redd096.StateMachine.StateMachineRedd096;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    #region timers vars

    [Header("Timers")]
    [SerializeField] int secondsBeforeStartTimer = 3;
    [SerializeField] int timerInSeconds = 90;
    [SerializeField] bool activePlayersAfterCountdown = true;
    [EnableIf("activePlayersAfterCountdown")][SerializeField] string firstStatePlayers = "NormalState";

    enum EStateLevelManager { countdown, game, endGame}

    EStateLevelManager state = EStateLevelManager.countdown;
    float timer;

    #endregion

    DraggableObject[] draggableObjectsInScene = default;
    TriggerZone[] triggerZonesInScene = default;

    private void Start()
    {
        ResetTimers();

        //save elements in scene
        draggableObjectsInScene = FindObjectsOfType<DraggableObject>();
        triggerZonesInScene = FindObjectsOfType<TriggerZone>();
    }

    private void Update()
    {
        UpdateTimers();
    }

    void OnWin()
    {
        state = EStateLevelManager.endGame;
        GameManager.uiManager.UpdateEndMenuText(true);
        GameManager.uiManager.EndMenu(true);
    }

    void OnFinishTimer()
    {
        //show end menu
        GameManager.uiManager.UpdateTimer(0);
        state = EStateLevelManager.endGame;
        GameManager.uiManager.UpdateEndMenuText(false);
        GameManager.uiManager.EndMenu(true);
    }

    #region timers

    void ResetTimers()
    {
        timer = Time.time + secondsBeforeStartTimer;
        GameManager.uiManager.UpdateCountdownBeforeStart(secondsBeforeStartTimer);
        GameManager.uiManager.UpdateTimer(timerInSeconds);
        GameManager.uiManager.ShowCountdown(true);
    }

    void UpdateTimers()
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

    #endregion

    public void TryActivateCharacter(Character character)
    {
        if (state == EStateLevelManager.game)
            character.GetComponentInChildren<StateMachineRedd096>().SetState(firstStatePlayers);
    }

    public void CheckWin()
    {
        //get every draggable object in every trigger zone
        List<DraggableObject> draggablesInTrigger = new List<DraggableObject>();
        foreach (TriggerZone triggerZone in triggerZonesInScene)
        {
            foreach (DraggableObject draggable in triggerZone.ObjectsInside)
            {
                if (draggablesInTrigger.Contains(draggable) == false)
                    draggablesInTrigger.Add(draggable);
            }
        }

        //check the list contains every draggable in scene
        foreach (DraggableObject draggableObject in draggableObjectsInScene)
        {
            //else, do nothing
            if (draggablesInTrigger.Contains(draggableObject) == false)
                return;
        }

        //if contains every draggable, call for win
        OnWin();
    }
}
