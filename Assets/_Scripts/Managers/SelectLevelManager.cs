using Fusion;
using redd096;
using redd096.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelManager : MonoBehaviour
{
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
        //change scene online
        if (NetworkManager.instance)
        {
            if (NetworkManager.instance.Runner.IsServer)
                NetworkManager.instance.Runner.SetActiveScene(level);
        }
        //change scene normally
        else
        {
            SceneLoader.instance.LoadScene(level);
        }
    }

    [System.Serializable]
    struct LevelButtonStruct
    {
        public Button button;
        [Scene] public string level;
    }
}
