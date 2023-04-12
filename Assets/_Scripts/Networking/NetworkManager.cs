using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using redd096;

public class NetworkManager : Singleton<NetworkManager>, INetworkRunnerCallbacks
{
    public NetworkRunner Runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    public string playerName;

    private List<SessionInfo> _sessions;
    public List<SessionInfo> Sessions
    {
        get { return _sessions; }
        private set { _sessions = value; }
    }

    void Start()
    {
        // Create the Fusion runner and let it know that we will be providing user input
        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;
    }

    public async void StartGame(GameMode mode, string sessionName, string username)
    {
        playerName = username;

        // Start or join (depends on gamemode) a session with a specific name
        await Runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = 1,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()            
        });
    }

    


    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OH ALLORA");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OH NO");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("UMMM");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("WOOOOO");
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars so we can remove it when they disconnect
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Find and remove the players avatar
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Update Session List");
        Sessions = sessionList;
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
