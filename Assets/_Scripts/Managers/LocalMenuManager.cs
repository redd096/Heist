using redd096.Attributes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LocalMenuManager : MonoBehaviour
{
    [SerializeField] GameObject localLobbyPlayerPrefab = default;
    [SerializeField] Transform container = default;
    [Scene][SerializeField] string sceneToLoadOnBack = "MainMenu";

    Dictionary<PlayerInput, GameObject> players = new Dictionary<PlayerInput, GameObject>();

    private void Awake()
    {
        //destroy every child
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    private void OnEnable()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;
    }

    private void OnDisable()
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
        localLobbyPlayerPrefab.GetComponentInChildren<TextMeshProUGUI>().text = "Player " + obj.playerIndex;
        players.Add(obj, go);
    }

    private void OnPlayerLeft(PlayerInput obj)
    {
        //remove player from UI
        Destroy(players[obj].gameObject);
        players.Remove(obj);
    }

    public void Back()
    {
        //remove connected players
        PlayerController[] playersInScene = FindObjectsOfType<PlayerController>();
        for (int i = playersInScene.Length - 1; i >= 0; i--)
            Destroy(playersInScene[i].gameObject);

        //destroy player input manager
        Destroy(PlayerInputManager.instance.gameObject);

        //because we are going back to main menu
        SceneManager.LoadScene(sceneToLoadOnBack);
    }
}
