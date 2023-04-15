using redd096;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EveryButtonFeedbacks : Singleton<EveryButtonFeedbacks>
{
    protected override void InitializeSingleton()
    {
        base.InitializeSingleton();

        //set buttons in first scene
        SetButtonsInScene();

        //and register to re-set in every scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        SetButtonsInScene();
    }

    void SetButtonsInScene()
    {
        //add PlayOnClick to every button in scene
        foreach (Button button in FindObjectsOfType<Button>(transform))
            button.onClick.AddListener(() => SoundManager.instance.PlayOnClick());
    }
}