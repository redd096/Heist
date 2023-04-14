using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    #region timers vars

    [Header("Timers")]
    [SerializeField] int secondsBeforeStartTimer = 3;
    [SerializeField] int timerInSeconds = 90;

    enum EStateLevelManager { countdown, game, endGame}

    EStateLevelManager state = EStateLevelManager.countdown;
    float timer;

    #endregion

    [Header("Pawns")]
    [SerializeField] PlayerPawn playerPrefab = default;
    [SerializeField] PlayerPawn[] playersInScene = default;

    //win check
    DraggableObject[] draggableObjectsInScene = default;
    TriggerZone[] triggerZonesInScene = default;

    private void Awake()
    {
        //online
        if (NetworkManager.instance)
        {
            NetworkManager.instance.OnSceneLoadDoneCallback += OnSceneLoadStartCallback;
        }
    }

    private void OnDestroy()
    {
        //online
        if (NetworkManager.instance)
        {
            NetworkManager.instance.OnSceneLoadDoneCallback -= OnSceneLoadStartCallback;
        }
    }

    private void Start()
    {
        ResetTimers();

        //save elements in scene
        draggableObjectsInScene = FindObjectsOfType<DraggableObject>();
        triggerZonesInScene = FindObjectsOfType<TriggerZone>();

        //local
        if (NetworkManager.instance == null)
        {
            //activate pawns in scene for every player
            foreach (PlayerController playerController in FindObjectsOfType<PlayerController>())
                TryActivatePlayer(playerController);
        }
    }

    void OnSceneLoadStartCallback(NetworkRunner runner)
    {
        //activate pawns in scene for every player
        foreach (PlayerController playerController in FindObjectsOfType<PlayerController>())
            TryActivatePlayer(playerController);
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

        //possess pawns
        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            TryActivatePlayer(player);
        }
    }

    #endregion

    public void TryActivatePlayer(PlayerController player)
    {
        PlayerPawn pawn = GetCharacterToPossess(player.playerIndex);
        if (pawn) pawn.gameObject.SetActive(true);

        if (state == EStateLevelManager.game)
            player.Possess(pawn);
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

    public PlayerPawn GetCharacterToPossess(int playerIndex)
    {
        if (playersInScene == null || playersInScene.Length <= 0)
            playersInScene = FindObjectsOfType<PlayerPawn>(true);

        //find player in the list
        if (playerIndex < playersInScene.Length && playersInScene[playerIndex] != null)
        {
            //be sure isn't already possessed
            if (playersInScene[playerIndex].CurrentController == null)
            {
                return playersInScene[playerIndex];
            }
        }

        //else instantiate new player
        PlayerPawn pawn = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        List<PlayerPawn> pawnsList = new List<PlayerPawn>(playersInScene);
        pawnsList.Add(pawn);
        playersInScene = pawnsList.ToArray();   //and add to the list
        return pawn;
    }
}
