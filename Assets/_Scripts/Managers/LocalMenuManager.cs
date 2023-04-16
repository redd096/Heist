using redd096;
using redd096.Attributes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalMenuManager : MonoBehaviour
{
    [SerializeField] GameObject localLobbyPlayerPrefab = default;
    [SerializeField] Transform container = default;
    [Scene][SerializeField] string sceneToLoadOnBack = "MainMenu";
    [SerializeField] Button selectLevelButton = default;
    [SerializeField] GameObject objectWhenNoPlayersInScene = default;

    Dictionary<PlayerInput, GameObject> players = new Dictionary<PlayerInput, GameObject>();

    private void Awake()
    {
        //destroy every child
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            //ignore object when no players in scene
            if (container.GetChild(i).gameObject == objectWhenNoPlayersInScene)
                continue;

            Destroy(container.GetChild(i).gameObject);
        }

        //disable select level button by default
        UpdateButtonInteractable();
    }

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;

        //set default players
        foreach (PlayerController p in FindObjectsOfType<PlayerController>())
            OnPlayerJoined(p.GetComponent<PlayerInput>());
    }

    private void OnDestroy()
    {
        if (PlayerInputManager.instance)
        {
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
            PlayerInputManager.instance.onPlayerLeft -= OnPlayerLeft;
        }
    }

    private void OnPlayerJoined(PlayerInput obj)
    {
        //show new player in UI
        GameObject go = Instantiate(localLobbyPlayerPrefab, container);
        go.GetComponentInChildren<TextMeshProUGUI>().text = "Player " + (obj.playerIndex + 1);
        go.GetComponentInChildren<Image>().color = GameManager.instance.PlayersColors[obj.playerIndex];
        players.Add(obj, go);
        if (objectWhenNoPlayersInScene) objectWhenNoPlayersInScene.SetActive(false);    //do immediatly, without wait update button

        //is enable if at least one player is in the scene
        Invoke(nameof(UpdateButtonInteractable), 0.1f);
    }

    private void OnPlayerLeft(PlayerInput obj)
    {
        //remove player from UI
        Destroy(players[obj].gameObject);
        players.Remove(obj);

        UpdateButtonInteractable();
    }

    public void Back()
    {
        //destroy player input manager
        Destroy(PlayerInputManager.instance.gameObject);

        //remove connected players
        PlayerController[] playersInScene = FindObjectsOfType<PlayerController>();
        for (int i = playersInScene.Length - 1; i >= 0; i--)
            Destroy(playersInScene[i].gameObject);

        //because we are going back to main menu
        SceneChangerAnimation.FadeOutLoadScene(sceneToLoadOnBack);
    }

    void UpdateButtonInteractable()
    {
        bool thereArePlayersInScene = players.Count > 0;
        if (objectWhenNoPlayersInScene) objectWhenNoPlayersInScene.SetActive(thereArePlayersInScene == false);
        if (selectLevelButton) selectLevelButton.interactable = thereArePlayersInScene;
    }
}
