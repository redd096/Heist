using Fusion;
using redd096.Attributes;
using redd096.GameTopDown2D;
using redd096.StateMachine.StateMachineRedd096;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPawn : Character
{
    [ReadOnly] public PlayerController CurrentController = default;

    PlayerInput _playerInput;
    PlayerInput playerInput { get { 
            if (_playerInput == null && CurrentController) 
                _playerInput = CurrentController.GetComponent<PlayerInput>(); 
            return _playerInput; } }

    NetworkInputData myInputs = new NetworkInputData();

    private void Awake()
    {
        //deactive statemachine if we are online but we are not the server
        if (NetworkManager.instance && NetworkManager.instance.Runner.IsServer == false)
            GetComponentInChildren<StateMachineRedd096>().enabled = false;
    }

    public override void Spawned()
    {
        base.Spawned();

        //find controller
        foreach (var controller in FindObjectsOfType<PlayerController>())
            if (controller.GetComponent<User>().Object.InputAuthority == Object.InputAuthority)
                controller.Possess(this);

        if (Object.HasInputAuthority)
            NetworkManager.instance.OnInputCallback += OnInput;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        if (Object.HasInputAuthority)
            NetworkManager.instance.OnInputCallback -= OnInput;
    }

    private void Update()
    {
        //check in update if press inputs
        if (CurrentController)
        {
            if (playerInput.actions.FindAction("Interact").WasPressedThisFrame())
                myInputs.buttons.Set(MyButtons.Interact, true);

            if (playerInput.actions.FindAction("Throw").WasPressedThisFrame())
                myInputs.buttons.Set(MyButtons.Throw, true);

            myInputs.move = playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        }
    }

    private void OnInput(NetworkInput input)
    {
        input.Set(myInputs);

        // Reset the input struct to start with a clean slate
        // when polling for the next tick
        myInputs = default;
    }
}

public enum MyButtons
{
    Interact = 0,
    Throw = 1,
}

public struct NetworkInputData : INetworkInput
{
    public NetworkButtons buttons;
    public Vector2 move;
}
