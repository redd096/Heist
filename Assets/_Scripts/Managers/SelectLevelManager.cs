using Fusion;
using redd096;
using redd096.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelManager : MonoBehaviour
{
    [Scene][SerializeField] string backSceneOnline = "Lobby";
    [Scene][SerializeField] string backSceneOffline = "LocalMenu";
    [SerializeField] LevelButtonStruct[] levelButtons = default;

    private void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            string s = levelButtons[i].level;
            levelButtons[i].button.onClick.AddListener(() => LoadLevel(s));
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
    }
}
