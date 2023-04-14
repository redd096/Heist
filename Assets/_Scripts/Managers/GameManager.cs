using redd096;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : Singleton<GameManager>
{
    public static UIManager uiManager { get; private set; }
    public static LevelManager levelManager { get; private set; }
    public static PauseManager pauseManager { get; private set; }

    public static List<RandomizeColors.ColorStruct> remainingColors = new List<RandomizeColors.ColorStruct>();
    public static List<User> usersInScene = new List<User>();

    protected override void SetDefaults()
    {
        base.SetDefaults();

        //set references
        uiManager = FindObjectOfType<UIManager>();
        levelManager = FindObjectOfType<LevelManager>();
        pauseManager = FindObjectOfType<PauseManager>();
    }
}
