using redd096;
using redd096.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelManager : MonoBehaviour
{
    [Scene][SerializeField] string backSceneOnline = "Lobby";
    [Scene][SerializeField] string backSceneOffline = "LocalMenu";
    [SerializeField] Button backButton = default;
    [SerializeField] LevelButtonStruct[] levelButtons = default;

    [Button(ButtonAttribute.EEnableType.PlayMode)] void UnlockAllLevels() { foreach (var v in levelButtons) v.button.interactable = true; }

    private void Start()
    {
        //deactive if not server
        backButton.interactable = NetworkManager.instance.Runner.IsServer;

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
        if (NetworkManager.instance.Runner.IsServer)
            SceneChangerAnimation.FadeOutLoadScene(level);
    }

    public void BackButton()
    {
        //change scene if online or offline
        if (NetworkManager.instance)
        {
            if (NetworkManager.instance.Runner.IsServer)
            {
                SceneChangerAnimation.FadeOutLoadScene(backSceneOnline);
            }
        }
        else
        {
            SceneChangerAnimation.FadeOutLoadScene(backSceneOffline);
        }
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
