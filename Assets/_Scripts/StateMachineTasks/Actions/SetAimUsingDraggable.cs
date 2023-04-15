using redd096.StateMachine.StateMachineRedd096;
using UnityEngine;

public class SetAimUsingDraggable : ActionTask
{
    [Header("Necessary Components - default get in parent")]
    [SerializeField] AimComponent aimComponent = default;
    [SerializeField] DragComponent dragComponent = default;

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
        if (dragComponent == null) dragComponent = GetStateMachineComponent<DragComponent>();
    }

    protected override void OnEnterTask()
    {
        base.OnEnterTask();

        if (aimComponent == null || dragComponent == null)
            return;
        
        Vector3 direction = -dragComponent.PossibleToPickRaycastHit.normal;
        aimComponent.AimInDirection(new Vector3(direction.x, 0, direction.z));
    }
}
