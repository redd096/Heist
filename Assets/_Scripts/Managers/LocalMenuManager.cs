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

    Dictionary<PlayerInput, GameObject> players = new Dictionary<PlayerInput, GameObject>();

    private void Awake()
    {
        //destroy every child
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

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
        //remove connected players
        PlayerController[] playersInScene = FindObjectsOfType<PlayerController>();
        for (int i = playersInScene.Length - 1; i >= 0; i--)
            Destroy(playersInScene[i].gameObject);

        //destroy player input manager
        Destroy(PlayerInputManager.instance.gameObject);

        //because we are going back to main menu
        SceneManager.LoadScene(sceneToLoadOnBack);
    }

    void UpdateButtonInteractable()
    {
        selectLevelButton.interactable = players.Count > 0;
    }
}
