using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

public class MoveByInput : ActionTask
{
    [Header("Necessary Components - default get in parent")]
    [SerializeField] MovementComponent movementComponent = default;
    [SerializeField] PlayerPawn pawn = default;

    [Header("Movement")]
    [SerializeField] string inputName = "Move";

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (movementComponent == null) movementComponent = GetStateMachineComponent<MovementComponent>();
        if (pawn == null) pawn = GetStateMachineComponent<PlayerPawn>();
    }

    public override void OnUpdateTask()
    {
        base.OnUpdateTask();

        if (NetworkManager.instance)
            return;

        if (movementComponent == null || pawn == null || pawn.CurrentController == null)
            return;

        //move in direction
        Vector2 direction = pawn.CurrentController.FindAction(inputName).ReadValue<Vector2>();
        movementComponent.MoveInDirection(new Vector3(direction.x, 0, direction.y));
    }

    public override void OnFixedUpdateNetworkTask()
    {
        base.OnFixedUpdateNetworkTask();

        if (movementComponent == null)
            return;

        //move in direction
        if (GetInput(out NetworkInputData input))
        {
            Vector2 direction = input.move;
            movementComponent.MoveInDirection(new Vector3(direction.x, 0, direction.y));
        }
    }
}