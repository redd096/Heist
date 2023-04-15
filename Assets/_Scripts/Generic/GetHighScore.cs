using redd096;
using redd096.Attributes;
using TMPro;
using UnityEngine;

public class GetHighScore : MonoBehaviour
{
    [Header("Score for current scene or select a scene to use")]
    [SerializeField] bool useCurrentScene = false;
    [Scene][SerializeField] string sceneOfHighScore = "LevelTest1";

    [Header("Set text where {0} is the HighScore")]
    [SerializeField] TextMeshProUGUI text = default;
    [SerializeField] string textToShow = "HIGHSCORE {0} !!";

    [Header("If there isn't an high score, deactive text")]
    [SerializeField] GameObject objectToDeactiveWhenScoreIsZero = default;

    public const string HIGHSCORE_SAVE = "HighScore";
    string sceneToUse => useCurrentScene ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name : sceneOfHighScore;

    private void OnEnable()
    {
        UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        int highScore = SaveManager.PlayerPrefsFWMV.GetInt(HIGHSCORE_SAVE, sceneToUse, 0);

        //set text with score
        if (text)
        {
            text.text = string.Format(textToShow, highScore.ToString());
        }

        //deactive object if there is no score
        if (highScore <= 0)
        {
            if (objectToDeactiveWhenScoreIsZero)
                objectToDeactiveWhenScoreIsZero.SetActive(false);
        }
    }

    [Button(ButtonAttribute.EEnableType.PlayMode)]
    void Add1ToScore() { SaveManager.PlayerPrefsFWMV.SetInt(HIGHSCORE_SAVE, sceneToUse, SaveManager.PlayerPrefsFWMV.GetInt(HIGHSCORE_SAVE, sceneToUse, 0) + 1); UpdateScoreText(); }
    [Button(ButtonAttribute.EEnableType.PlayMode)]
    void Add10ToScore() { SaveManager.PlayerPrefsFWMV.SetInt(HIGHSCORE_SAVE, sceneToUse, SaveManager.PlayerPrefsFWMV.GetInt(HIGHSCORE_SAVE, sceneToUse, 0) + 10); UpdateScoreText(); }
    [Button(ButtonAttribute.EEnableType.PlayMode)]
    void RemoveScore() { SaveManager.PlayerPrefsFWMV.DeleteKey(HIGHSCORE_SAVE, sceneToUse); UpdateScoreText(); }
}
