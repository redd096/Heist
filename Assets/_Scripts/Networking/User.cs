using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class User : NetworkBehaviour
{
    [Networked]
    public string Username { get; set; }

    private void Start()
    {
        if(Object.HasInputAuthority)
            Username = NetworkManager.instance.playerName;
    }
}
