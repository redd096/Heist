using UnityEngine;
using UnityEngine.UI;

public class RandomizeColors : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] Image backgroundImage = default;

    [Header("Interactions - if empty, get every Selectable in scene")]
    [SerializeField] Selectable[] interactionsInScene = default;

    [Header("Colors")]
    [SerializeField] ColorStruct[] randomColors = default;

    void Awake()
    {
        //get random color
        ColorStruct randomColor = GetRandomColors();

        //set background color
        backgroundImage.color = randomColor.backgroundColor;

        //set interactions in scene
        Selectable[] selectables = interactionsInScene != null && interactionsInScene.Length > 0 ? interactionsInScene : FindObjectsOfType<Selectable>(true);
        foreach (var v in selectables)
        {
            ColorBlock colorBlock = v.colors;
            colorBlock.normalColor = randomColor.normalColor;
            colorBlock.highlightedColor = randomColor.highlightedColor;
            colorBlock.pressedColor = randomColor.pressedColor;
            colorBlock.selectedColor = randomColor.selectedColor;
            colorBlock.disabledColor = randomColor.disabledColor;
            v.colors = colorBlock;
        }
    }

    ColorStruct GetRandomColors()
    {
        //when reach 0, recreate list
        if (GameManager.instance.remainingColors.Count <= 0)
            GameManager.instance.remainingColors.AddRange(randomColors);

        //select random color and remove from the list
        int index = Random.Range(0, GameManager.instance.remainingColors.Count);
        ColorStruct foundColor = GameManager.instance.remainingColors[index];
        GameManager.instance.remainingColors.RemoveAt(index);

        return foundColor;
    }

    [System.Serializable]
    public class ColorStruct
    {
        public Color backgroundColor;
        [Space]
        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color selectedColor;
        public Color disabledColor;
    }
}