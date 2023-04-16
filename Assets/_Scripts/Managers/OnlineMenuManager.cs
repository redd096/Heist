using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using System.Linq;
using redd096.Attributes;
using UnityEngine.SceneManagement;
using redd096;

public class OnlineMenuManager : MonoBehaviour
{
    [Scene][SerializeField] string backButtonScene = "MainMenu";
    [Scene][SerializeField] int lobbyScene = 3;
    public TMP_InputField roomCode;
    public TMP_InputField username;
    public TMP_Text joinErrorText;
    public string errorMessage;

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
        NetworkManager.instance.OnStartCreateOrJoinLobby += OnStartJoinRoom;
        NetworkManager.instance.OnCompleteCreateOrJoinRoom += OnCompleteJoinRoom;
    }

    private void OnStartJoinRoom()
    {
        joinErrorText.text = "";
    }

    private void OnCompleteJoinRoom(bool isSuccess)
    {
        if (!isSuccess)
        {
            SceneChangerAnimation.FadeIn();
            joinErrorText.text = errorMessage;
        }
    }

    public void Create()
    {
        //if no name, set one random
        if (username.text == "")
            username.text = fantasyNames[Random.Range(0, fantasyNames.Length)];

        //start game
        var id = GenerateRoomID();
        SceneChangerAnimation.FadeOut();
        NetworkManager.instance.StartGame(GameMode.Host, id, username.text, lobbyScene);
    }

    private string GenerateRoomID()
    {
        string result;

        //generate random string, check no sessions has same string, else retry
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
        //if no name, set one random
        if (username.text == "")
            username.text = fantasyNames[Random.Range(0, fantasyNames.Length)];

        SceneChangerAnimation.FadeOut();
        //start game
        NetworkManager.instance.StartGame(GameMode.Client, roomCode.text.ToUpper(), username.text, lobbyScene);
    }

    public void BackButton()
    {
        //back
        Destroy(NetworkManager.instance.gameObject);

        SceneChangerAnimation.FadeOutLoadScene(backButtonScene);
    }

    private void OnDestroy()
    {
        if(NetworkManager.instance != null)
        {
            NetworkManager.instance.OnStartCreateOrJoinLobby -= OnStartJoinRoom;
            NetworkManager.instance.OnCompleteCreateOrJoinRoom -= OnCompleteJoinRoom;
        }
    }
}
