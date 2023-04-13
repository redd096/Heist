using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Fusion.Sockets;
using System;

public class Lobby : MonoBehaviour
{
    public TMP_Text text;
    public GameObject playerPrefab;
    public Transform container;
    private Dictionary<string, GameObject> _players = new Dictionary<string, GameObject>();

    private void Awake()
    {
        //destroy every child
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        NetworkManager.instance.OnPlayerEnter += AddPlayer;
        NetworkManager.instance.OnPlayerRefreshName += RefreshPlayer;
        NetworkManager.instance.OnPlayerExit += RemovePlayer;
    }

    public void Start()
    {
        text.text = NetworkManager.instance.Runner.SessionInfo.Name;
    }

    public void AddPlayer(User user)
    {
        var go = Instantiate(playerPrefab, container);
        go.GetComponentInChildren<TMP_Text>().text = user.Username;
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

    }

    public void GoToSelectLevel()
    {

    }
}
