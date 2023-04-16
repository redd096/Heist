using UnityEngine;
using redd096.Attributes;
using redd096;
using redd096.StateMachine.StateMachineRedd096;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    [Header("If pause menu has sub-menus (like Options), set which one is the first menu to know when resume game")]
    [SerializeField] GameObject pauseMenuFirstSelectedMenu = default;

    [Header("Scenes on click Exit")]
    [Scene][SerializeField] string normalBackLevel = "SelectLevel";
    [Scene][SerializeField] string backLevelOnlineClient = "OnlineMenu";

    public bool IsPlaying { get; private set; } = true;

    Dictionary<PlayerController, string> previousStates = new Dictionary<PlayerController, string>();

    private void OnEnable()
    {
        if (GameManager.levelManager)
            GameManager.levelManager.onChangeState += OnLevelManagerChangeState;
    }

    private void OnDisable()
    {
        if (GameManager.levelManager)
            GameManager.levelManager.onChangeState += OnLevelManagerChangeState;
    }

    void OnLevelManagerChangeState()
    {
        //be sure to not have pause menu active, when end game
        if (GameManager.levelManager.State == LevelManager.EStateLevelManager.endGame)
            Resume();
    }

    public void Pause()
    {
        //only in game
        if (GameManager.levelManager.State != LevelManager.EStateLevelManager.game)
            return;

        IsPlaying = false;

        //show pause menu
        GameManager.uiManager.PauseMenu(true);

        //online stop only self player
        if (NetworkManager.instance)
        {
            foreach (PlayerController playerController in GameManager.playersInScene)
                if (playerController.GetComponent<User>().Object.InputAuthority == NetworkManager.instance.Runner.LocalPlayer)
                    if (playerController.CurrentPawn)
                    {
                        StateMachineRedd096 sm = playerController.CurrentPawn.GetComponentInChildren<StateMachineRedd096>();
                        previousStates.Add(playerController, sm.GetCurrentState());
                        sm.SetState(-1);
                    }
        }
        //local, stop every player
        else
        {
            foreach (PlayerController playerController in GameManager.playersInScene)
                if (playerController.CurrentPawn)
                {
                    StateMachineRedd096 sm = playerController.CurrentPawn.GetComponentInChildren<StateMachineRedd096>();
                    previousStates.Add(playerController, sm.GetCurrentState());
                    sm.SetState(-1);
                }
        }
    }

    public void Resume()
    {
        //only if in the first menu of pause menu
        if (pauseMenuFirstSelectedMenu.activeSelf)
        {
            IsPlaying = true;

            //hide pause menu
            GameManager.uiManager.PauseMenu(false);

            //resume states
            foreach (PlayerController playerController in previousStates.Keys)
            {
                if (playerController.CurrentPawn)
                {
                    StateMachineRedd096 sm = playerController.CurrentPawn.GetComponentInChildren<StateMachineRedd096>();
                    sm.SetState(previousStates[playerController]);
                }
            }
            previousStates.Clear();
        }
    }

    public void ExitButton()
    {
        //online but client, leave room
        if (NetworkManager.instance && NetworkManager.instance.Runner.IsServer == false)
        {
            NetworkManager.instance.LeaveGame();
            SceneChangerAnimation.FadeOutLoadScene(backLevelOnlineClient);
        }
        //if offline, or server, just back to select level
        else
        {
            SceneChangerAnimation.FadeOutLoadScene(normalBackLevel);
        }
    }
}
