using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;
using redd096.GameTopDown2D;
using UnityEngine.InputSystem;

public class InteractByInput : ActionTask
{
    [Header("Necessary Components - default get in parent")]
    [SerializeField] DragComponent interactComponent = default;
    [SerializeField] PlayerPawn pawn = default;

    [Header("Interact")]
    [SerializeField] string inputName = "Interact";

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (interactComponent == null) interactComponent = GetStateMachineComponent<DragComponent>();
        if (pawn == null) pawn = GetStateMachineComponent<PlayerPawn>();
    }

    public override void OnUpdateTask()
    {
        base.OnUpdateTask();

        if (NetworkManager.instance)
            return;

        if (interactComponent == null || pawn == null || pawn.CurrentController == null)
            return;

        //when press input, interact
        if (pawn.CurrentController.FindAction(inputName).WasPressedThisFrame())
            interactComponent.Interact();
    }

    public override void OnFixedUpdateNetworkTask()
    {
        base.OnFixedUpdateNetworkTask();

        if (interactComponent == null || pawn == null || pawn.CurrentController == null)
            return;

        if (GetInput(out NetworkInputData input))
        {
            if (input.buttons.IsSet(MyButtons.Interact))
                interactComponent.Interact();
        }
    }
}
