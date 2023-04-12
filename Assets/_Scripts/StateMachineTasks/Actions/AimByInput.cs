using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

public class AimByInput : ActionTask
{
    [Header("Necessary Components - default get in parent")]
    [SerializeField] AimComponent aimComponent = default;
    [SerializeField] PlayerPawn pawn = default;

    [Header("Aim")]
    [SerializeField] string inputName = "Move";

    Vector2 inputValue;

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
        if (pawn == null) pawn = GetStateMachineComponent<PlayerPawn>();
    }

    public override void OnUpdateTask()
    {
        base.OnUpdateTask();

        if (aimComponent == null || pawn == null || pawn.CurrentController == null)
            return;

        //get input
        inputValue = pawn.CurrentController.FindAction(inputName).ReadValue<Vector2>();

        //check if moving analog or reset input when released
        aimComponent.AimInDirection(new Vector3(inputValue.x, 0, inputValue.y));
    }
}