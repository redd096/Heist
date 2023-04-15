using redd096;
using redd096.Attributes;
using TMPro;
using UnityEngine;

public class GetHighScore : MonoBehaviour
{
    [Header("Scene of this high score")]
    [Scene][SerializeField] string sceneOfHighScore = "LevelTest1";

    [Header("Set text where {0} is the HighScore")]
    [SerializeField] TextMeshProUGUI text = default;
    [SerializeField] string textToShow = "HIGHSCORE {0} !!";

    [Header("If there isn't an high score, deactive text")]
    [SerializeField] GameObject objectToDeactiveWhenScoreIsZero = default;

    public const string HIGHSCORE_SAVE = "HighScore";

    private void OnEnable()
    {
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        float highScore = SaveManager.PlayerPrefsFWMV.GetFloat(HIGHSCORE_SAVE, sceneOfHighScore, 0);

        //set text with score
        if (text)
        {
            text.text = string.Format(textToShow, highScore.ToString("F2"));
        }

        //deactive object if there is no score
        if (highScore <= Mathf.Epsilon)
        {
            if (objectToDeactiveWhenScoreIsZero)
                objectToDeactiveWhenScoreIsZero.SetActive(false);
        }
    }

    [Button(ButtonAttribute.EEnableType.PlayMode)]
    void Add001ToScore() { SaveManager.PlayerPrefsFWMV.SetFloat(HIGHSCORE_SAVE, sceneOfHighScore, SaveManager.PlayerPrefs.GetFloat(HIGHSCORE_SAVE, 0) + 0.01f); UpdateScoreText(); }
    [Button(ButtonAttribute.EEnableType.PlayMode)]
    void Add01ToScore() { SaveManager.PlayerPrefsFWMV.SetFloat(HIGHSCORE_SAVE, sceneOfHighScore, SaveManager.PlayerPrefs.GetFloat(HIGHSCORE_SAVE, 0) + 0.1f); UpdateScoreText(); }
    [Button(ButtonAttribute.EEnableType.PlayMode)]
    void Add1ToScore() { SaveManager.PlayerPrefsFWMV.SetFloat(HIGHSCORE_SAVE, sceneOfHighScore, SaveManager.PlayerPrefs.GetFloat(HIGHSCORE_SAVE, 0) + 1f); UpdateScoreText(); }
    [Button(ButtonAttribute.EEnableType.PlayMode)]
    void RemoveScore() { SaveManager.PlayerPrefsFWMV.DeleteKey(HIGHSCORE_SAVE, sceneOfHighScore); UpdateScoreText(); }
}
