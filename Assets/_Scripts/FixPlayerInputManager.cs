using redd096.GameTopDown2D;
using UnityEngine;
using UnityEngine.InputSystem;

public class FixPlayerInputManager : MonoBehaviour
{
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = transform.position;
        GameManager.levelManager.TryActivateCharacter(playerInput.GetComponent<Character>());
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {

    }
}
