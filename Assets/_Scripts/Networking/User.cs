using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class User : NetworkBehaviour
{
    [Networked(OnChanged = nameof(RefreshUI))]
    public string Username { get; set; }


    public override void Spawned()
    {
        NetworkManager.instance.OnPlayerEnter?.Invoke(this);
        if (Object.HasInputAuthority)
        {
            RPC_SendName(NetworkManager.instance.playerName);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SendName(string username, RpcInfo info = default)
    {
        Username = username;
    }

    public static void RefreshUI(Changed<User> changed)
    {
        NetworkManager.instance.OnPlayerRefreshName?.Invoke(changed.Behaviour);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        NetworkManager.instance.OnPlayerRefreshName?.Invoke(this);
        base.Despawned(runner, hasState);
    }
}
