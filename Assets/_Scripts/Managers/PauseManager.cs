using UnityEngine;
using redd096.Attributes;
using redd096;

public class PauseManager : MonoBehaviour
{
    [Header("If pause menu has sub-menus (like Options), set which one is the first menu to know when resume game")]
    [SerializeField] GameObject pauseMenuFirstSelectedMenu = default;

    [Header("Stop time when paused")]
    [SerializeField] bool stopTimeWhenPaused = true;

    [Header("Scenes on click Exit")]
    [Scene][SerializeField] string normalBackLevel = "SelectLevel";
    [Scene][SerializeField] string backLevelOnlineClient = "OnlineMenu";

    public bool IsPlaying { get; private set; } = true;

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

        //stop time
        if (stopTimeWhenPaused)
            Time.timeScale = 0;
    }

    public void Resume()
    {
        //only if in the first menu of pause menu
        if (pauseMenuFirstSelectedMenu.activeSelf)
        {
            IsPlaying = true;

            //hide pause menu
            GameManager.uiManager.PauseMenu(false);

            //set timeScale to 1
            if (stopTimeWhenPaused)
                Time.timeScale = 1;
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
