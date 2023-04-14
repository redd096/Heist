using Fusion;
using redd096.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [ReadOnly] public PlayerPawn CurrentPawn = default;

    public int playerIndex => playerInput.playerIndex;

    PlayerInput _playerInput;
    PlayerInput playerInput { get { if (_playerInput == null) _playerInput = GetComponent<PlayerInput>(); return _playerInput; } }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        //check if in a scene with pause manager
        if (GameManager.pauseManager)
        {
            if (GameManager.pauseManager.IsPlaying)
            {
                //check if press Pause
                if (playerInput.actions.FindAction("Pause").WasPressedThisFrame())
                {
                    GameManager.pauseManager.Pause();
                }
            }
            else
            {
                //check if press resume
                if (playerInput.actions.FindAction("Resume").WasPressedThisFrame())
                {
                    GameManager.pauseManager.Resume();
                }
            }
        }
    }

    public void Possess(PlayerPawn pawn)
    {
        Unpossess();            //be sure to possess only one pawn
        CurrentPawn = pawn;
        if (CurrentPawn)
        {
            if (CurrentPawn.CurrentController) CurrentPawn.CurrentController.Unpossess();   //if another controller is possessing pawn, unpossess
            CurrentPawn.CurrentController = this;                                           //possess pawn
        }

        //if online, set input authority
        if (NetworkManager.instance)
            CurrentPawn.GetComponent<NetworkObject>().AssignInputAuthority(GetComponent<User>().Object.InputAuthority);
    }

    public void Unpossess()
    {
        if (CurrentPawn)
        {
            CurrentPawn.CurrentController = null;       //unpossess
            CurrentPawn = null;                         //set pawn null
        }
    }

    public InputAction FindAction(string action)
    {
        return playerInput.actions.FindAction(action);
    }
}
