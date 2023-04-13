using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using System.Linq;

public class MainMenuManager : MonoBehaviour
{
    public TMP_InputField roomCode;
    public TMP_InputField username;

    string[] fantasyNames = new string[50] {"Tharion", "Eryndor", "Arintha", "Kaelin", "Eldrid",
                      "Draven", "Ryker", "Torin", "Lirien", "Galadrielle",
                      "Valoria", "Zephyr", "Sable", "Celestia", "Nyx",
                      "Calantha", "Sorin", "Isadora", "Auriel", "Thalia",
                      "Gwyneth", "Lorien", "Alaric", "Rowan", "Eira",
                      "Darian", "Seraphina", "Evander", "Lysandra", "Halcyon",
                      "Thorne", "Rhiannon", "Kairos", "Sabriel", "Cygnus",
                      "Caius", "Elara", "Orion", "Lyra", "Serenity",
                      "Daedalus", "Lyris", "Vesper", "Aeloria", "Cassius",
                      "Xanthe", "Thetis", "Zarek", "Niamh", "Elwyn" };

    private void Start()
    {
        //set random name
        username.text = fantasyNames[Random.Range(0, fantasyNames.Length)];
    }

    public void Create()
    {
        if (username.text == "")
            return;

        var id = GenerateRoomID();
        NetworkManager.instance.StartGame(GameMode.Host, id, username.text);
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
        if (username.text == "")
            return;

        NetworkManager.instance.StartGame(GameMode.Client, roomCode.text.ToUpper(), username.text);
    }
}
