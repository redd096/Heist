using UnityEngine;
using UnityEngine.InputSystem;

public class FixPlayerInputManager : MonoBehaviour
{
    enum EState { waitingToBeInitialized, CanBeUsed, Destroyed }
    EState currentState = EState.waitingToBeInitialized;

    private void Start()
    {
        //in normal game, we have player input manager only in the local lobby, without this script
        //but in gameplay scenes, we have it with this script, just to test rapidly

        if (Check())
        {
            PlayerInputManager.instance.EnableJoining();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        //playerInput.transform.position = transform.position;

        //call if the player is istantiated after the countdown.
        //If the countodown is still running, this do nothing and the player will be activated normally
        if (Check())
            GameManager.levelManager.TryActivatePlayer(playerInput.GetComponent<PlayerController>());
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (Check())
            Destroy(playerInput.gameObject);
    }

    private bool Check()
    {
        if (currentState == EState.CanBeUsed)
            return true;
        if (currentState == EState.Destroyed)
            return false;

        //if we are in gameplay scene, and we are not online and there aren't players in scene, then this is the first scene, so we are testing in editor
        if (GameManager.instance && GameManager.levelManager && NetworkManager.instance == null && FindObjectsOfType<PlayerController>().Length <= 0)
        {
            currentState = EState.CanBeUsed;
            return true;
        }
        //else, we are playing normally, and we don't need this in scene
        else
        {
            currentState = EState.Destroyed;
            return false;
        }
    }
}
