using redd096.Attributes;
using TMPro;
using UnityEngine;

public class GetCurrentScore : MonoBehaviour
{
    [HelpBox("QUESTO FUNZIONA SOLO IN SCENE CON IL LEVEL MANAGER", HelpBoxAttribute.EMessageType.Warning)]
    [Header("Set text where {0} is the CurrentScore")]
    [SerializeField] TextMeshProUGUI text = default;
    [SerializeField] string textToShow = "Score: {0}";

    [Header("If there isn't a score, deactive text")]
    [SerializeField] GameObject objectToDeactiveWhenScoreIsZero = default;

    private void OnEnable()
    {
        UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        int score = GameManager.levelManager.Score;

        //set text with score
        if (text)
        {
            text.text = string.Format(textToShow, score.ToString());
        }

        //deactive object if there is no score
        if (score <= Mathf.Epsilon)
        {
            if (objectToDeactiveWhenScoreIsZero)
                objectToDeactiveWhenScoreIsZero.SetActive(false);
        }
    }
}
