using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using redd096;

public class MenuManager : Singleton<MenuManager>
{
    public TMP_InputField roomCode;
    public NetworkRunner _runner;

    async void StartGame(GameMode mode, string sessionName)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = 1,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void Create()
    {
        var id = "00000";
        Debug.Log(id);
        StartGame(GameMode.Host, id);
    }

    public void Join()
    {
        StartGame(GameMode.Client, roomCode.text);
    }
}
