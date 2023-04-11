using redd096;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : Singleton<GameManager>
{
    public static UIManager uiManager { get; private set; }
    public static LevelManager levelManager { get; private set; }

    protected override void SetDefaults()
    {
        base.SetDefaults();

        //set references
        uiManager = FindObjectOfType<UIManager>();
        levelManager = FindObjectOfType<LevelManager>();
    }
}
