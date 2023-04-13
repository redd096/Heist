using UnityEngine;
using UnityEngine.InputSystem;

public class FixPlayerInputManager : MonoBehaviour
{
    private void Start()
    {
        //in normal game, we have player input manager only in the local lobby, without this script
        //but in gameplay scenes, we have it with this script, just to test rapidly
        if (GameManager.instance && GameManager.levelManager && PlayerInputManager.instance == this && NetworkManager.instance == null)
        {
            //if we are in gameplay scene, and this is the instance. Then this is the first scene, so we are testing in editor
            PlayerInputManager.instance.EnableJoining();
        }
        else
        {
            //else, we are playing normally, and we don't need this in scene
            Destroy(gameObject);
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        //playerInput.transform.position = transform.position;

        //call if the player is istantiated after the countdown.
        //If the countodown is still running, this do nothing and the player will be activated normally
        if (GameManager.instance && GameManager.levelManager && PlayerInputManager.instance == this && NetworkManager.instance == null)
            GameManager.levelManager.TryActivatePlayer(playerInput.GetComponent<PlayerController>());
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (GameManager.instance && GameManager.levelManager && PlayerInputManager.instance == this && NetworkManager.instance == null)
            Destroy(playerInput.gameObject);
    }
}
