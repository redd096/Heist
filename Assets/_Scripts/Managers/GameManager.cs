using redd096;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : Singleton<GameManager>
{
    [Header("Players Colors")]
    public Color[] PlayersColors = default;

    public static UIManager uiManager { get; private set; }
    public static LevelManager levelManager { get; private set; }
    public static PauseManager pauseManager { get; private set; }

    public static List<RandomizeColors.ColorStruct> remainingColors = new List<RandomizeColors.ColorStruct>();

    protected override void SetDefaults()
    {
        base.SetDefaults();

        //set references
        uiManager = FindObjectOfType<UIManager>();
        levelManager = FindObjectOfType<LevelManager>();
        pauseManager = FindObjectOfType<PauseManager>();
    }
}
