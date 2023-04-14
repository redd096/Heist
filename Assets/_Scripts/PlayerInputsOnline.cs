using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputsOnline : NetworkBehaviour
{
    PlayerInput _playerInput;
    PlayerInput playerInput
    {
        get
        {
            if (_playerInput == null)
                _playerInput = GetComponent<PlayerInput>();
            return _playerInput;
        }
    }

    NetworkInputData myInputs = new NetworkInputData();

    public override void Spawned()
    {
        base.Spawned();

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
        //check in update if press inputs (for multiplayer online)
        if (NetworkManager.instance)
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
