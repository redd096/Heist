using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using System.Linq;

public class MainMenuManager : MonoBehaviour
{
    public TMP_InputField roomCode;

    public void Create()
    {
        var id = GenerateRoomID();
        NetworkManager.instance.StartGame(GameMode.Host, id);
    }

    private string GenerateRoomID()
    {
        string result;
        while (true)
        {
            result = RandomString(4);

            if (NetworkManager.instance.Sessions == null)
                break;

            if (!NetworkManager.instance.Sessions.Find((room) => room.Name == result))
                break;

        };
        return result;
    }

    public static string RandomString(int length)
    {
        System.Random rand = new System.Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
    }

    public void Join()
    {
        NetworkManager.instance.StartGame(GameMode.Client, roomCode.text);
    }
}
