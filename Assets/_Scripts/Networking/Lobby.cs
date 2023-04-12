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


    public void Start()
    {
        text.text = NetworkManager.instance.Runner.SessionInfo.Name;
    }
}
