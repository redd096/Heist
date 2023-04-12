using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

public class ThrowByInput : ActionTask
{
    [Header("Necessary Components - default get in parent")]
    [SerializeField] ThrowComponent throwComponent = default;
    [SerializeField] PlayerPawn pawn = default;

    [Header("Throw")]
    [SerializeField] string inputName = "Throw";

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (throwComponent == null) throwComponent = GetStateMachineComponent<ThrowComponent>();
        if (pawn == null) pawn = GetStateMachineComponent<PlayerPawn>();
    }

    public override void OnUpdateTask()
    {
        base.OnUpdateTask();

        if (throwComponent == null || pawn == null || pawn.CurrentController == null)
            return;

        //when press input
        if (pawn.CurrentController.FindAction(inputName).WasPressedThisFrame())
            throwComponent.Throw();
    }
}
