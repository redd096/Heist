using UnityEngine;
using UnityEngine.InputSystem;

public class FixPlayerInputManager : MonoBehaviour
{
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        //playerInput.transform.position = transform.position;

        //call if the player is istantiated after the countdown.
        //If the countodown is still running, this do nothing and the player will be activated normally
        GameManager.levelManager.TryActivatePlayer(playerInput.GetComponent<PlayerController>());
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Destroy(playerInput.gameObject);
    }
}
