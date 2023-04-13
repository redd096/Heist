using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMenuManager : MonoBehaviour
{
    [SerializeField] GameObject localLobbyPlayerPrefab = default;
    [SerializeField] Transform container = default;

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
        PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft -= OnPlayerLeft;
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
}
