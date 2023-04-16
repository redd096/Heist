using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using redd096.Attributes;
using redd096;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    public TMP_Text text;
    public GameObject playerPrefab;
    public Transform container;
    [Scene] public string sceneOnBack = "OnlineMenu";
    [SerializeField] Button selectLevelButton = default;
    [Scene] public string sceneOnSelectLevel = "SelectLevel";

    private Dictionary<string, GameObject> _players = new Dictionary<string, GameObject>();

    private void Awake()
    {
        //destroy every child
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        //deactive if not server
        selectLevelButton.interactable = NetworkManager.instance.Runner.IsServer;

        NetworkManager.instance.OnPlayerEnter += AddPlayer;
        NetworkManager.instance.OnPlayerRefreshName += RefreshPlayer;
        NetworkManager.instance.OnPlayerExit += RemovePlayer;

        //set default players
        foreach (User u in FindObjectsOfType<User>())
            AddPlayer(u);
    }

    public void Start()
    {
        text.text = NetworkManager.instance.Runner.SessionInfo.Name;
    }

    public void AddPlayer(User user)
    {
        var go = Instantiate(playerPrefab, container);
        go.GetComponentInChildren<TMP_Text>().text = user.Username;
        go.GetComponentInChildren<Image>().color = GameManager.instance.PlayersColors[user.GetComponent<PlayerInput>().playerIndex];
        _players.Add(user.Object.Id.ToString(), go);
    }

    public void RefreshPlayer(User user)
    {
        if(!_players.ContainsKey(user.Object.Id.ToString()))
            return;

        var go = _players[user.Object.Id.ToString()];
        go.GetComponentInChildren<TMP_Text>().text = user.Username;
    }

    public void RemovePlayer(User user)
    {
        var go = _players[user.Object.Id.ToString()];
        _players.Remove(user.Object.Id.ToString());
        Destroy(go);
    }

    public void Back()
    {
        //leave online
        NetworkManager.instance.LeaveGame();
        SceneChangerAnimation.instance.FadeOutLoadScene(sceneOnBack);
    }

    public void GoToSelectLevel()
    {
        //only server can call this button
        SceneChangerAnimation.instance.FadeOutLoadScene(sceneOnSelectLevel);
    }
}
