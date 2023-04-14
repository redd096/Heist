using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("If pause menu has sub-menus (like Options), set which one is the first menu to know when resume game")]
    [SerializeField] GameObject pauseMenuFirstSelectedMenu = default;

    [Header("Stop time when paused")]
    [SerializeField] bool stopTimeWhenPaused = true;

    public bool IsPlaying { get; private set; } = true;

    public void Pause()
    {
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
}
