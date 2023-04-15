using redd096;
using redd096.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelManager : MonoBehaviour
{
    [Scene][SerializeField] string backSceneOnline = "Lobby";
    [Scene][SerializeField] string backSceneOffline = "LocalMenu";
    [SerializeField] LevelButtonStruct[] levelButtons = default;

    [Button(ButtonAttribute.EEnableType.PlayMode)] void UnlockAllLevels() { foreach (var v in levelButtons) v.button.interactable = true; }

    private void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            string s = levelButtons[i].level;
            levelButtons[i].button.onClick.AddListener(() => LoadLevel(s));

            int scoreInLevelToCheck = SaveManager.PlayerPrefsFWMV.GetInt(GetHighScore.HIGHSCORE_SAVE, levelButtons[i].levelToCheckScoreForUnlock, 0);
            levelButtons[i].button.interactable = scoreInLevelToCheck >= levelButtons[i].minScoreToUnlock;
        }
    }

    void LoadLevel(string level)
    {
        SceneLoader.instance.LoadScene(level);
    }

    public void BackButton()
    {
        if (NetworkManager.instance)
            SceneLoader.instance.LoadScene(backSceneOnline);
        else
            SceneLoader.instance.LoadScene(backSceneOffline);
    }

    [System.Serializable]
    struct LevelButtonStruct
    {
        public Button button;
        [Scene] public string level;
        [Min(0)] public int minScoreToUnlock;
        [Scene] public string levelToCheckScoreForUnlock;
    }
}
